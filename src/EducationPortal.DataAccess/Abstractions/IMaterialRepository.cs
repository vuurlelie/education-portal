using EducationPortal.DataAccess.Entities;

namespace EducationPortal.DataAccess.Abstractions;

public interface IMaterialRepository
{
    Task<Material?> GetByIdAsync(int materialId, CancellationToken cancellationToken = default);
    Task<Material?> GetWithDetailsByIdAsync(int materialId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Material>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Material material, CancellationToken cancellationToken = default);
    void Update(Material material);
    Task<bool> DeleteByIdAsync(int materialId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BookFormat>> GetBookFormatsAsync(CancellationToken cancellationToken = default);
}