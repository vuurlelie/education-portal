using EducationPortal.DataAccess.Abstractions;
using EducationPortal.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace EducationPortal.DataAccess.Repositories;

public sealed class UserMaterialRepository : IUserMaterialRepository
{
    private readonly IAppDbContext _databaseContext;

    public UserMaterialRepository(IAppDbContext databaseContext)
    {
        _databaseContext = databaseContext;
    }

    public async Task<bool> ExistsAsync(Guid userId, int materialId, CancellationToken cancellationToken = default)
    {
        return await _databaseContext.UserMaterials
            .AnyAsync(userMaterial =>
                userMaterial.UserId == userId &&
                userMaterial.MaterialId == materialId &&
                userMaterial.RecordStatus == RecordStatus.Active,
                cancellationToken);
    }

    public async Task AddAsync(UserMaterial userMaterial, CancellationToken cancellationToken = default)
    {
        await _databaseContext.UserMaterials.AddAsync(userMaterial, cancellationToken);
    }

    public async Task<IReadOnlyList<UserMaterial>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _databaseContext.UserMaterials
            .Where(userMaterial =>
                userMaterial.UserId == userId &&
                userMaterial.RecordStatus == RecordStatus.Active)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<int>> GetCompletedMaterialIdsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _databaseContext.UserMaterials
            .Where(userMaterial =>
                userMaterial.UserId == userId &&
                userMaterial.RecordStatus == RecordStatus.Active)
            .Select(userMaterial => userMaterial.MaterialId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task<UserMaterial?> GetByUserAndMaterialAsync(Guid userId, int materialId, CancellationToken cancellationToken = default)
    {
        return await _databaseContext.UserMaterials
            .Include(userMaterial => userMaterial.User)
            .Include(userMaterial => userMaterial.Material)
            .SingleOrDefaultAsync(userMaterial => userMaterial.User.Id == userId && userMaterial.Material.Id == materialId, cancellationToken);
    }

    public async Task<int> CountCompletedByUserForMaterialIdsAsync(Guid userId, IReadOnlyCollection<int> materialIds, CancellationToken cancellationToken = default)
    {
        if (materialIds is null || materialIds.Count == 0)
        {
            return 0;
        }

        return await _databaseContext.UserMaterials
            .Where(userMaterial => userMaterial.User.Id == userId && materialIds.Contains(userMaterial.Material.Id))
            .CountAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<int>> GetCompletedMaterialIdsByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _databaseContext.UserMaterials
            .Where(link => link.UserId == userId)
            .Select(link => link.MaterialId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public Task<bool> AnyByMaterialIdAsync(int materialId, CancellationToken cancellationToken = default)
    {
        return _databaseContext.UserMaterials
            .AsNoTracking()
            .AnyAsync(userMaterial => userMaterial.MaterialId == materialId && userMaterial.RecordStatus == RecordStatus.Active, cancellationToken);
    }
}