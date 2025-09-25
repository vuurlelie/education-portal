using EducationPortal.DataAccess.Abstractions;
using EducationPortal.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace EducationPortal.DataAccess.Repositories;

public sealed class MaterialRepository : IMaterialRepository
{
    private readonly IAppDbContext _databaseContext;

    public MaterialRepository(IAppDbContext databaseContext)
    {
        _databaseContext = databaseContext;
    }

    public async Task<Material?> GetByIdAsync(int materialId, CancellationToken cancellationToken = default)
    {
        return await _databaseContext.Materials
            .SingleOrDefaultAsync(material => material.Id == materialId, cancellationToken);
    }

    public async Task<Material?> GetWithDetailsByIdAsync(int materialId, CancellationToken cancellationToken = default)
    {
        return await _databaseContext.Materials
            .Include(material => material.CourseMaterials)
            .SingleOrDefaultAsync(material => material.Id == materialId, cancellationToken);
    }

    public async Task<IReadOnlyList<BookFormat>> GetBookFormatsAsync(CancellationToken cancellationToken = default)
    {
        return await _databaseContext.BookFormats
            .AsNoTracking()
            .OrderBy(format => format.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Material>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var materials = await _databaseContext.Materials
            .Include(material => (material as BookMaterial)!.Format)
            .AsNoTracking()
            .OrderBy(material => material.Title)
            .ToListAsync(cancellationToken);

        return materials;
    }

    public async Task AddAsync(Material material, CancellationToken cancellationToken = default)
    {
        await _databaseContext.Materials.AddAsync(material, cancellationToken);
    }

    public void Update(Material material)
    {
        _databaseContext.Materials.Update(material);
    }

    public async Task<bool> DeleteByIdAsync(int materialId, CancellationToken cancellationToken = default)
    {
        var material = await _databaseContext.Materials
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(material => material.Id == materialId, cancellationToken);

        if (material is null || material.RecordStatus == RecordStatus.Deleted)
        {
            return false;
        }

        material.RecordStatus = RecordStatus.Deleted;
        _databaseContext.Materials.Update(material);

        return true;
    }
}