using Jotwai.Domain.RequestModels;
using Jotwai.Domain.ValidateModel;
using Jotwai.Domain.ViewModels;

namespace Jotwai.Domain.Interfaces;

public interface ICategoriesService
{
    Task<CategoryWriteResult> CreateAsync(Guid userId, CreateCategoryRequest request, CancellationToken cancellationToken = default);

    Task<CategoryResponse?> GetByIdAsync(Guid userId, Guid categoryId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CategoryResponse>> ListAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<CategoryWriteResult> RenameAsync(Guid userId, Guid categoryId, RenameCategoryRequest request, CancellationToken cancellationToken = default);

    /// <summary>Deleting a Category has no "still in use" guard (unlike Topic) - Note-Category
    /// attachments are just dropped (ticket 023 has no such rule).</summary>
    Task<bool> DeleteAsync(Guid userId, Guid categoryId, CancellationToken cancellationToken = default);
}
