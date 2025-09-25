using EducationPortal.BusinessLogic.Abstractions;
using EducationPortal.BusinessLogic.DTOs.Courses;
using EducationPortal.BusinessLogic.Mappers;
using EducationPortal.DataAccess.Abstractions;
using EducationPortal.DataAccess.Entities;

namespace EducationPortal.BusinessLogic.Services;

public sealed class CourseService : ICourseService
{
    private readonly IUnitOfWork _unitOfWork;

    public CourseService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<CourseListItemDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var courses = await _unitOfWork.CourseRepository.GetAllAsync(cancellationToken);
        var items = courses.ToListItemDtos();
        return items;
    }

    public async Task<CourseDetailsDto?> GetDetailsAsync(int courseId, CancellationToken cancellationToken = default)
    {
        var course = await _unitOfWork.CourseRepository.GetWithDetailsByIdAsync(courseId, cancellationToken);
        if (course is null)
        {
            return null;
        }

        var details = course.ToDetailsDto();
        return details;
    }

    public async Task<int> CreateAsync(CourseCreateDto newCourse, CancellationToken cancellationToken = default)
    {
        var entity = newCourse.ToEntity();

        await _unitOfWork.CourseRepository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }

    public async Task UpdateAsync(int courseId, CourseEditDto changes, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.CourseRepository.GetByIdAsync(courseId, cancellationToken);
        if (entity is null)
        {
            throw new KeyNotFoundException($"Course {courseId} not found.");
        }

        changes.ApplyChanges(entity);

        _unitOfWork.CourseRepository.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync(int courseId, CancellationToken cancellationToken = default)
    {
        var inUse = await _unitOfWork.UserCourseRepository.AnyActiveByCourseIdAsync(courseId, cancellationToken);
        if (inUse)
        {
            throw new InvalidOperationException("This course cannot be deleted because users are already enrolled (in progress or completed).");
        }

        var deleted = await _unitOfWork.CourseRepository.DeleteByIdAsync(courseId, cancellationToken);
        if (deleted)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return deleted;
    }

    public async Task UpdateCourseMaterialsAsync(int courseId, IReadOnlyCollection<int> materialIds, CancellationToken cancellationToken = default)
    {
        var course = await _unitOfWork.CourseRepository.GetWithDetailsByIdAsync(courseId, cancellationToken);
        if (course is null)
        {
            throw new KeyNotFoundException($"Course {courseId} not found.");
        }

        var desiredMaterialIds = materialIds.Distinct().ToHashSet();
        var existingLinks = course.CourseMaterials.ToList();

        foreach (var link in existingLinks)
        {
            var shouldBeActive = desiredMaterialIds.Contains(link.MaterialId);
            var isActive = link.RecordStatus == RecordStatus.Active;

            link.RecordStatus = (isActive, shouldBeActive) switch
            {
                (true, false) => RecordStatus.Deleted,
                (false, true) => RecordStatus.Active,
                _ => link.RecordStatus
            };
        }

        var existingMaterialIdSet = existingLinks.Select(link => link.MaterialId).ToHashSet();
        var missingMaterialIds = desiredMaterialIds.Except(existingMaterialIdSet);

        foreach (var materialId in missingMaterialIds)
        {
            course.CourseMaterials.Add(new CourseMaterial
            {
                CourseId = courseId,
                MaterialId = materialId,
                RecordStatus = RecordStatus.Active
            });
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateCourseSkillsAsync(int courseId, IReadOnlyCollection<int> skillIds, CancellationToken cancellationToken = default)
    {
        var course = await _unitOfWork.CourseRepository.GetWithDetailsByIdAsync(courseId, cancellationToken);
        if (course is null)
        {
            throw new KeyNotFoundException($"Course {courseId} not found.");
        }

        var desiredSkillIds = skillIds.Distinct().ToHashSet();
        var existingLinks = course.CourseSkills.ToList();

        foreach (var link in existingLinks)
        {
            var shouldBeActive = desiredSkillIds.Contains(link.SkillId);
            var isActive = link.RecordStatus == RecordStatus.Active;

            link.RecordStatus = (isActive, shouldBeActive) switch
            {
                (true, false) => RecordStatus.Deleted,
                (false, true) => RecordStatus.Active,
                _ => link.RecordStatus
            };
        }

        var existingSkillIdSet = existingLinks.Select(link => link.SkillId).ToHashSet();
        var missingSkillIds = desiredSkillIds.Except(existingSkillIdSet);

        foreach (var skillId in missingSkillIds)
        {
            course.CourseSkills.Add(new CourseSkill
            {
                CourseId = courseId,
                SkillId = skillId,
                RecordStatus = RecordStatus.Active
            });
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}