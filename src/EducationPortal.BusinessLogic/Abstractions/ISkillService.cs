using EducationPortal.BusinessLogic.DTOs.Skills;

namespace EducationPortal.BusinessLogic.Abstractions;

public interface ISkillService
{
    Task<IReadOnlyList<SkillListItemDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<SkillDetailsDto?> GetDetailsAsync(int skillId, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(SkillCreateDto newSkill, CancellationToken cancellationToken = default);
    Task UpdateAsync(int skillId, SkillEditDto changes, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int skillId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<int>> GetAssignedCourseIdsAsync(int skillId, CancellationToken cancellationToken = default);
    Task UpdateSkillCoursesAsync(int skillId, IReadOnlyCollection<int> courseIds, CancellationToken cancellationToken = default);
}