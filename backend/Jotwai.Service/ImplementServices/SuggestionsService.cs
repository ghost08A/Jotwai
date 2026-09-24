using Jotwai.Domain.Events;
using Jotwai.Domain.Interfaces;
using Jotwai.Domain.Models;
using Jotwai.Domain.Models.Database;
using Jotwai.Domain.RequestModels;
using Jotwai.Domain.ValidateModel;
using Jotwai.Domain.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Jotwai.Service.ImplementServices;

public sealed class SuggestionsService(
    AiCategorizationDbContext db,
    INotesService notesService,
    IEventPublisher eventPublisher) : ISuggestionsService
{
    public async Task<SuggestionResponse?> GetActiveAsync(Guid userId, Guid noteId, CancellationToken cancellationToken = default)
    {
        var suggestion = await db.Suggestions.AsNoTracking()
            .SingleOrDefaultAsync(s => s.NoteId == noteId && s.UserId == userId && s.ResolvedAt == null, cancellationToken);
        if (suggestion is null)
            return null;

        var candidates = await db.SuggestionCandidates.AsNoTracking()
            .Where(c => c.SuggestionId == suggestion.Id)
            .OrderBy(c => c.Rank)
            .ToListAsync(cancellationToken);

        return ToResponse(suggestion, candidates);
    }

    public async Task<bool> IgnoreAsync(Guid userId, Guid noteId, CancellationToken cancellationToken = default)
    {
        var suggestion = await db.Suggestions.SingleOrDefaultAsync(
            s => s.NoteId == noteId && s.UserId == userId && s.ResolvedAt == null, cancellationToken);
        if (suggestion is null)
            return false;

        suggestion.ResolvedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        return true;
    }

    // Bulk-accept is one atomic transaction across three schemas (ticket 010): creating any
    // new Categories (owned by Organization) and attaching every accepted Category to the
    // Note (owned by Notes) must succeed or fail together. ADR-0002's Domain Events are
    // fire-and-forget notifications, not a fit for that - a true cross-schema transaction
    // needs all three DbContexts sharing one physical connection (EF Core's UseTransaction
    // requires this), which the DI-injected, independently-pooled DbContexts don't do. So
    // this builds its own connection/transaction and constructs Notes/Organization service
    // instances bound to it, reusing their existing validation logic. Domain Events for the
    // change are only published *after* the transaction commits - publishing mid-transaction
    // would let a handler's own (separately-connected) write commit independently and go
    // stale if this transaction then rolled back.
    public async Task<AcceptSuggestionResult> AcceptAsync(Guid userId, Guid noteId, AcceptSuggestionRequest request, CancellationToken cancellationToken = default)
    {
        var suggestion = await db.Suggestions.AsNoTracking()
            .SingleOrDefaultAsync(s => s.NoteId == noteId && s.UserId == userId && s.ResolvedAt == null, cancellationToken);
        if (suggestion is null)
            return AcceptSuggestionResult.Failure(AcceptSuggestionFailureReason.SuggestionNotFound);

        var candidates = await db.SuggestionCandidates.AsNoTracking()
            .Where(c => c.SuggestionId == suggestion.Id)
            .ToListAsync(cancellationToken);

        var candidateIds = candidates.Select(c => c.Id).ToHashSet();
        var requestedIds = request.CandidateIds.Distinct().ToList();
        if (requestedIds.Any(id => !candidateIds.Contains(id)))
            return AcceptSuggestionResult.Failure(AcceptSuggestionFailureReason.InvalidCandidateIds);

        var accepted = candidates.Where(c => requestedIds.Contains(c.Id)).ToList();

        // Reads the connection string off the already-configured DbContext, not IConfiguration
        // directly - so this keeps working when a test host points DbContexts at a different
        // database (e.g. a Testcontainers instance) via DI, not via config.
        var connectionString = db.Database.GetConnectionString()
            ?? throw new InvalidOperationException("AiCategorizationDbContext has no connection string.");

        await using var connection = new Npgsql.NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        await using var notesTx = new NotesDbContext(new DbContextOptionsBuilder<NotesDbContext>().UseNpgsql(connection).Options);
        await notesTx.Database.UseTransactionAsync(transaction, cancellationToken);
        await using var organizationTx = new OrganizationDbContext(new DbContextOptionsBuilder<OrganizationDbContext>().UseNpgsql(connection).Options);
        await organizationTx.Database.UseTransactionAsync(transaction, cancellationToken);
        await using var aiTx = new AiCategorizationDbContext(new DbContextOptionsBuilder<AiCategorizationDbContext>().UseNpgsql(connection).Options);
        await aiTx.Database.UseTransactionAsync(transaction, cancellationToken);

        var newlyCreatedCategories = new List<Category>();
        var attachedCategoryIds = new List<Guid>();

        foreach (var candidate in accepted)
        {
            Guid categoryId;

            if (candidate.CategoryId is { } existingId)
            {
                var stillExists = await organizationTx.Categories.AnyAsync(c => c.Id == existingId && c.UserId == userId, cancellationToken);
                if (!stillExists)
                    return AcceptSuggestionResult.Failure(AcceptSuggestionFailureReason.InvalidCandidateIds);

                categoryId = existingId;
            }
            else
            {
                var name = candidate.NewCategoryName!.Trim();
                var normalizedName = name.ToLowerInvariant();

                // Someone may have created a Category with this exact name in the meantime -
                // reuse it instead of violating the uniqueness rule (ticket 028).
                var existing = await organizationTx.Categories.FirstOrDefaultAsync(
                    c => c.UserId == userId && c.NormalizedName == normalizedName, cancellationToken);

                if (existing is not null)
                {
                    categoryId = existing.Id;
                }
                else
                {
                    var now = DateTimeOffset.UtcNow;
                    var newCategory = new Category
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        Name = name,
                        NormalizedName = normalizedName,
                        CreatedAt = now,
                        UpdatedAt = now,
                    };
                    organizationTx.Categories.Add(newCategory);
                    newlyCreatedCategories.Add(newCategory);
                    categoryId = newCategory.Id;
                }
            }

            var alreadyAttached = await notesTx.NoteCategories.AnyAsync(nc => nc.NoteId == noteId && nc.CategoryId == categoryId, cancellationToken);
            if (!alreadyAttached)
            {
                notesTx.NoteCategories.Add(new NoteCategory { NoteId = noteId, CategoryId = categoryId });
                attachedCategoryIds.Add(categoryId);
            }
        }

        await organizationTx.SaveChangesAsync(cancellationToken);
        await notesTx.SaveChangesAsync(cancellationToken);

        await aiTx.Suggestions
            .Where(s => s.Id == suggestion.Id)
            .ExecuteUpdateAsync(setters => setters.SetProperty(s => s.ResolvedAt, DateTimeOffset.UtcNow), cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        foreach (var category in newlyCreatedCategories)
            await eventPublisher.PublishAsync(new CategoryCreated(category.Id, category.UserId, category.Name), cancellationToken);

        var attachedAt = DateTimeOffset.UtcNow;
        foreach (var categoryId in attachedCategoryIds)
            await eventPublisher.PublishAsync(new NoteCategoryAttached(noteId, userId, categoryId, attachedAt), cancellationToken);

        var note = await notesService.GetByIdAsync(userId, noteId, cancellationToken)
            ?? throw new InvalidOperationException("Note vanished mid-transaction.");

        return AcceptSuggestionResult.Success(note);
    }

    private static SuggestionResponse ToResponse(Suggestion suggestion, List<SuggestionCandidate> candidates) => new()
    {
        Id = suggestion.Id,
        NoteId = suggestion.NoteId,
        CreatedAt = suggestion.CreatedAt,
        Candidates = candidates.Select(c => new SuggestionCandidateResponse
        {
            Id = c.Id,
            CategoryId = c.CategoryId,
            NewCategoryName = c.NewCategoryName,
            Rank = c.Rank,
            Confidence = c.Confidence,
        }).ToList(),
    };
}
