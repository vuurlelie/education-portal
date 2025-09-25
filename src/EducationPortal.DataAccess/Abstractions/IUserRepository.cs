using EducationPortal.DataAccess.Entities;

namespace EducationPortal.DataAccess.Abstractions;

public interface IUserRepository
{
    Task<ApplicationUser?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ApplicationUser>> GetByIdsAsync(IReadOnlyList<Guid> userIds, CancellationToken cancellationToken = default);
}