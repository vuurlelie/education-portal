using EducationPortal.BusinessLogic.Abstractions;
using EducationPortal.BusinessLogic.DTOs.Skills;
using EducationPortal.BusinessLogic.Mappers;
using EducationPortal.DataAccess.Abstractions;
using EducationPortal.DataAccess.Entities;

namespace EducationPortal.BusinessLogic.Services;

public sealed class SkillService : ISkillService
{
    private readonly IUnitOfWork _unitOfWork;

    public SkillService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<SkillListItemDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var skills = await _unitOfWork.SkillRepository.GetAllAsync(cancellationToken);
        var items = skills.ToListItemDtos();
        return items;
    }

    public async Task<SkillDetailsDto?> GetDetailsAsync(int skillId, CancellationToken cancellationToken = default)
    {
        var skill = await _unitOfWork.SkillRepository.GetWithDetailsByIdAsync(skillId, cancellationToken);
        if (skill is null)
        {
            return null;
        }

        var details = skill.ToDetailsDto();
        return details;
    }

    public async Task<int> CreateAsync(SkillCreateDto newSkill, CancellationToken cancellationToken = default)
    {
        var entity = newSkill.ToEntity();

        await _unitOfWork.SkillRepository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }

    public async Task UpdateAsync(int skillId, SkillEditDto changes, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.SkillRepository.GetByIdAsync(skillId, cancellationToken)
                    ?? throw new KeyNotFoundException($"Skill {skillId} not found.");

        changes.ApplyChanges(entity);

        _unitOfWork.SkillRepository.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync(int skillId, CancellationToken cancellationToken = default)
    {
        var inUse = await _unitOfWork.UserSkillRepository.AnyBySkillIdAsync(skillId, cancellationToken);
        if (inUse)
        {
            throw new InvalidOperationException("This skill cannot be deleted because it has already been awarded to users.");
        }

        var deleted = await _unitOfWork.SkillRepository.DeleteByIdAsync(skillId, cancellationToken);
        if (deleted)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return deleted;
    }

    public async Task<IReadOnlyList<int>> GetAssignedCourseIdsAsync(int skillId, CancellationToken cancellationToken = default)
    {
        var skillWithLinks = await _unitOfWork.SkillRepository.GetWithDetailsByIdAsync(skillId, cancellationToken)
                            ?? throw new KeyNotFoundException($"Skill {skillId} not found.");

        var courseIds = skillWithLinks.CourseSkills
            .Where(link => link.RecordStatus == RecordStatus.Active)
            .Select(link => link.CourseId)
            .Distinct()
            .OrderBy(id => id)
            .ToList();

        return courseIds;
    }

    public async Task UpdateSkillCoursesAsync(int skillId, IReadOnlyCollection<int> courseIds, CancellationToken cancellationToken = default)
    {
        var skillWithLinks = await _unitOfWork.SkillRepository.GetWithDetailsByIdAsync(skillId, cancellationToken)
                            ?? throw new KeyNotFoundException($"Skill {skillId} not found.");

        var desiredCourseIds = courseIds.Distinct().ToHashSet();
        var existingLinks = skillWithLinks.CourseSkills.ToList();

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
            skillWithLinks.CourseSkills.Add(new CourseSkill
            {
                CourseId = newCourseId,
                SkillId = skillId,
                RecordStatus = RecordStatus.Active
            });
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}