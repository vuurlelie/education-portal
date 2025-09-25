using EducationPortal.DataAccess.Entities;

namespace EducationPortal.DataAccess.Abstractions;

public interface IUserMaterialRepository
{
    Task<bool> ExistsAsync(Guid userId, int materialId, CancellationToken cancellationToken = default);
    Task AddAsync(UserMaterial userMaterial, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserMaterial>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<int>> GetCompletedMaterialIdsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserMaterial?> GetByUserAndMaterialAsync(Guid userId, int materialId, CancellationToken cancellationToken = default);
    Task<int> CountCompletedByUserForMaterialIdsAsync(Guid userId, IReadOnlyCollection<int> materialIds, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<int>> GetCompletedMaterialIdsByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> AnyByMaterialIdAsync(int materialId, CancellationToken cancellationToken = default);
}