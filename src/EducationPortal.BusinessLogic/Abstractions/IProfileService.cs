using EducationPortal.BusinessLogic.DTOs.Profile;

namespace EducationPortal.BusinessLogic.Abstractions;

public interface IProfileService
{
    Task<UserProfileDto> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserCourseItemDto>> GetCoursesInProgressAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserCourseItemDto>> GetCompletedCoursesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserSkillItemDto>> GetSkillsAsync(Guid userId, CancellationToken cancellationToken = default);
}