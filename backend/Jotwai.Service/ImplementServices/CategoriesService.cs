using Jotwai.Domain.Events;
using Jotwai.Domain.Interfaces;
using Jotwai.Domain.Models;
using Jotwai.Domain.Models.Database;
using Jotwai.Domain.RequestModels;
using Jotwai.Domain.ValidateModel;
using Jotwai.Domain.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Jotwai.Service.ImplementServices;

public sealed class CategoriesService(OrganizationDbContext db, IEventPublisher eventPublisher) : ICategoriesService
{
    public async Task<CategoryWriteResult> CreateAsync(Guid userId, CreateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var name = request.Name.Trim();
        var normalizedName = Normalize(name);

        var duplicate = await db.Categories.AnyAsync(c => c.UserId == userId && c.NormalizedName == normalizedName, cancellationToken);
        if (duplicate)
            return CategoryWriteResult.Failure(CategoryWriteFailureReason.DuplicateName);

        var now = DateTimeOffset.UtcNow;
        var category = new Category
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = name,
            NormalizedName = normalizedName,
            CreatedAt = now,
            UpdatedAt = now,
        };

        db.Categories.Add(category);
        await db.SaveChangesAsync(cancellationToken);

        await eventPublisher.PublishAsync(new CategoryCreated(category.Id, category.UserId, category.Name), cancellationToken);

        return CategoryWriteResult.Success(ToResponse(category));
    }

    public async Task<CategoryResponse?> GetByIdAsync(Guid userId, Guid categoryId, CancellationToken cancellationToken = default)
    {
        var category = await db.Categories.AsNoTracking()
            .SingleOrDefaultAsync(c => c.Id == categoryId && c.UserId == userId, cancellationToken);

        return category is null ? null : ToResponse(category);
    }

    public async Task<IReadOnlyList<CategoryResponse>> ListAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var categories = await db.Categories.AsNoTracking()
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

        return categories.Select(ToResponse).ToList();
    }

    public async Task<CategoryWriteResult> RenameAsync(Guid userId, Guid categoryId, RenameCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var category = await db.Categories.SingleOrDefaultAsync(c => c.Id == categoryId && c.UserId == userId, cancellationToken);
        if (category is null)
            return CategoryWriteResult.Failure(CategoryWriteFailureReason.NotFound);

        var name = request.Name.Trim();
        var normalizedName = Normalize(name);

        var duplicate = await db.Categories.AnyAsync(
            c => c.Id != categoryId && c.UserId == userId && c.NormalizedName == normalizedName,
            cancellationToken);
        if (duplicate)
            return CategoryWriteResult.Failure(CategoryWriteFailureReason.DuplicateName);

        category.Name = name;
        category.NormalizedName = normalizedName;
        category.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        await eventPublisher.PublishAsync(new CategoryRenamed(category.Id, category.UserId, category.Name), cancellationToken);

        return CategoryWriteResult.Success(ToResponse(category));
    }

    public async Task<bool> DeleteAsync(Guid userId, Guid categoryId, CancellationToken cancellationToken = default)
    {
        var category = await db.Categories.SingleOrDefaultAsync(c => c.Id == categoryId && c.UserId == userId, cancellationToken);
        if (category is null)
            return false;

        db.Categories.Remove(category);
        await db.SaveChangesAsync(cancellationToken);

        await eventPublisher.PublishAsync(new CategoryDeleted(category.Id, category.UserId), cancellationToken);

        return true;
    }

    private static string Normalize(string name) => name.ToLowerInvariant();

    private static CategoryResponse ToResponse(Category category) => new()
    {
        Id = category.Id,
        Name = category.Name,
        CreatedAt = category.CreatedAt,
        UpdatedAt = category.UpdatedAt,
    };
}
