using EducationPortal.DataAccess.Abstractions;
using EducationPortal.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace EducationPortal.DataAccess.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly IAppDbContext _databaseContext;

    public UserRepository(IAppDbContext databaseContext)
    {
        _databaseContext = databaseContext;
    }

    public async Task<ApplicationUser?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _databaseContext.Users
            .SingleOrDefaultAsync(user => user.Id == userId, cancellationToken);
    }

    public async Task<IReadOnlyList<ApplicationUser>> GetByIdsAsync(IReadOnlyList<Guid> userIds, CancellationToken cancellationToken = default)
    {
        if (userIds.Count == 0)
        {
            return Array.Empty<ApplicationUser>();
        }

        var distinctIds = userIds.Distinct().ToArray();

        return await _databaseContext.Users
            .Where(user => distinctIds.Contains(user.Id))
            .ToListAsync(cancellationToken);
    }
}