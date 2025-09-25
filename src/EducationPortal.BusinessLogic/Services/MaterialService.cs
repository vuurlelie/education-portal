using EducationPortal.BusinessLogic.Abstractions;
using EducationPortal.BusinessLogic.DTOs.Materials;
using EducationPortal.BusinessLogic.Mappers;
using EducationPortal.DataAccess.Abstractions;
using EducationPortal.DataAccess.Entities;

namespace EducationPortal.BusinessLogic.Services;

public sealed class MaterialService : IMaterialService
{
    private readonly IUnitOfWork _unitOfWork;

    public MaterialService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<MaterialListItemDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var materials = await _unitOfWork.MaterialRepository.GetAllAsync(cancellationToken);
        var items = materials.ToListItemDtos();
        return items;
    }

    public async Task<MaterialDetailsDto?> GetDetailsAsync(int materialId, CancellationToken cancellationToken = default)
    {
        var material = await _unitOfWork.MaterialRepository.GetWithDetailsByIdAsync(materialId, cancellationToken);
        if (material is null)
        {
            return null;
        }

        var details = material.ToDetailsDto();
        return details;
    }

    public async Task<int> CreateVideoAsync(VideoMaterialCreateDto newVideo, CancellationToken cancellationToken = default)
    {
        var entity = newVideo.ToEntity();
        await _unitOfWork.MaterialRepository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }

    public async Task<int> CreateBookAsync(BookMaterialCreateDto newBook, CancellationToken cancellationToken = default)
    {
        var entity = newBook.ToEntity();
        await _unitOfWork.MaterialRepository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }

    public async Task<int> CreateArticleAsync(ArticleMaterialCreateDto newArticle, CancellationToken cancellationToken = default)
    {
        var entity = newArticle.ToEntity();
        await _unitOfWork.MaterialRepository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }

    public async Task UpdateVideoAsync(int materialId, VideoMaterialEditDto changes, CancellationToken cancellationToken = default)
    {
        var baseEntity = await _unitOfWork.MaterialRepository.GetByIdAsync(materialId, cancellationToken);
        if (baseEntity is null)
        {
            throw new KeyNotFoundException($"Material {materialId} not found.");
        }

        if (baseEntity is not VideoMaterial entity)
        {
            throw new InvalidOperationException($"Material {materialId} is not a video.");
        }

        changes.ApplyChanges(entity);

        _unitOfWork.MaterialRepository.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateBookAsync(int materialId, BookMaterialEditDto changes, CancellationToken cancellationToken = default)
    {
        var baseEntity = await _unitOfWork.MaterialRepository.GetByIdAsync(materialId, cancellationToken);
        if (baseEntity is null)
        {
            throw new KeyNotFoundException($"Material {materialId} not found.");
        }

        if (baseEntity is not BookMaterial entity)
        {
            throw new InvalidOperationException($"Material {materialId} is not a book.");
        }

        changes.ApplyChanges(entity);

        _unitOfWork.MaterialRepository.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateArticleAsync(int materialId, ArticleMaterialEditDto changes, CancellationToken cancellationToken = default)
    {
        var baseEntity = await _unitOfWork.MaterialRepository.GetByIdAsync(materialId, cancellationToken);
        if (baseEntity is null)
        {
            throw new KeyNotFoundException($"Material {materialId} not found.");
        }

        if (baseEntity is not ArticleMaterial entity)
        {
            throw new InvalidOperationException($"Material {materialId} is not an article.");
        }

        changes.ApplyChanges(entity);

        _unitOfWork.MaterialRepository.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync(int materialId, CancellationToken cancellationToken = default)
    {
        var inUse = await _unitOfWork.UserMaterialRepository.AnyByMaterialIdAsync(materialId, cancellationToken);
        if (inUse)
        {
            throw new InvalidOperationException("This material cannot be deleted because some users have already completed it.");
        }

        var deleted = await _unitOfWork.MaterialRepository.DeleteByIdAsync(materialId, cancellationToken);
        if (deleted)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return deleted;
    }

    public async Task<IReadOnlyList<(int Id, string Name)>> GetBookFormatsAsync(CancellationToken cancellationToken = default)
    {
        var formats = await _unitOfWork.MaterialRepository.GetBookFormatsAsync(cancellationToken);
        var items = formats.Select(format => (format.Id, format.Name)).ToList();
        return items;
    }

    public async Task<IReadOnlyList<int>> GetAssignedCourseIdsAsync(int materialId, CancellationToken cancellationToken = default)
    {
        var materialWithLinks = await _unitOfWork.MaterialRepository.GetWithDetailsByIdAsync(materialId, cancellationToken);
        if (materialWithLinks is null)
        {
            throw new KeyNotFoundException($"Material {materialId} not found.");
        }

        var assignedIds = materialWithLinks.CourseMaterials
            .Where(link => link.RecordStatus == RecordStatus.Active)
            .Select(link => link.CourseId)
            .Distinct()
            .ToList();

        return assignedIds;
    }

    public async Task UpdateMaterialCoursesAsync(int materialId, IReadOnlyCollection<int> courseIds, CancellationToken cancellationToken = default)
    {
        var materialWithLinks = await _unitOfWork.MaterialRepository.GetWithDetailsByIdAsync(materialId, cancellationToken);
        if (materialWithLinks is null)
        {
            throw new KeyNotFoundException($"Material {materialId} not found.");
        }

        var desiredCourseIds = courseIds.Distinct().ToHashSet();
        var existingLinks = materialWithLinks.CourseMaterials.ToList();

        foreach (var link in existingLinks)
        {
            var shouldBeActive = desiredCourseIds.Contains(link.CourseId);
            var isActive = link.RecordStatus == RecordStatus.Active;

            link.RecordStatus = (isActive, shouldBeActive) switch
            {
                (true, false) => RecordStatus.Deleted,
                (false, true) => RecordStatus.Active,
                _ => link.RecordStatus
            };
        }

        var existingCourseIdSet = existingLinks.Select(link => link.CourseId).ToHashSet();
        var missingCourseIds = desiredCourseIds.Except(existingCourseIdSet);

        foreach (var newCourseId in missingCourseIds)
        {
            materialWithLinks.CourseMaterials.Add(new CourseMaterial
            {
                CourseId = newCourseId,
                MaterialId = materialId,
                RecordStatus = RecordStatus.Active
            });
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}