using EducationPortal.BusinessLogic.DTOs.Materials;

namespace EducationPortal.BusinessLogic.Abstractions;

public interface IMaterialService
{
    Task<IReadOnlyList<MaterialListItemDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<MaterialDetailsDto?> GetDetailsAsync(int materialId, CancellationToken cancellationToken = default);

    Task<int> CreateVideoAsync(VideoMaterialCreateDto newVideo, CancellationToken cancellationToken = default);
    Task<int> CreateBookAsync(BookMaterialCreateDto newBook, CancellationToken cancellationToken = default);
    Task<int> CreateArticleAsync(ArticleMaterialCreateDto newArticle, CancellationToken cancellationToken = default);

    Task UpdateVideoAsync(int materialId, VideoMaterialEditDto changes, CancellationToken cancellationToken = default);
    Task UpdateBookAsync(int materialId, BookMaterialEditDto changes, CancellationToken cancellationToken = default);
    Task UpdateArticleAsync(int materialId, ArticleMaterialEditDto changes, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int materialId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<(int Id, string Name)>> GetBookFormatsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<int>> GetAssignedCourseIdsAsync(int materialId, CancellationToken cancellationToken = default);
    Task UpdateMaterialCoursesAsync(int materialId, IReadOnlyCollection<int> courseIds, CancellationToken cancellationToken = default);
}