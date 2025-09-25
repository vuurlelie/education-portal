using EducationPortal.BusinessLogic.Abstractions;
using EducationPortal.BusinessLogic.DTOs.Profile;
using EducationPortal.BusinessLogic.Mappers;
using EducationPortal.DataAccess.Abstractions;
using EducationPortal.DataAccess.Entities;

namespace EducationPortal.BusinessLogic.Services;

public sealed class ProfileService : IProfileService
{
    private readonly IUnitOfWork unitOfWork;

    public ProfileService(IUnitOfWork unitOfWork)
    {
        this.unitOfWork = unitOfWork;
    }

    public async Task<UserProfileDto> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await unitOfWork.UserRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            throw new KeyNotFoundException($"User {userId} not found.");
        }

        var userCourses = await unitOfWork.UserCourseRepository.GetByUserIdAsync(userId, cancellationToken);
        var userSkills = await unitOfWork.UserSkillRepository.GetByUserIdAsync(userId, cancellationToken);

        var completedCount = userCourses.Count(link => link.RecordStatus == RecordStatus.Active && link.ProgressPercent == BusinessRules.MaxProgressPercent);
        var inProgressCount = userCourses.Count(link => link.RecordStatus == RecordStatus.Active && link.ProgressPercent < BusinessRules.MaxProgressPercent);
        var activeSkillsCount = userSkills.Count(link => link.RecordStatus == RecordStatus.Active);

        var profile = user.ToUserProfileDto(inProgressCount, completedCount, activeSkillsCount);
        return profile;
    }

    public async Task<IReadOnlyList<UserCourseItemDto>> GetCoursesInProgressAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var userCourses = await unitOfWork.UserCourseRepository.GetByUserIdAsync(userId, cancellationToken);

        var inProgressLinks = userCourses
            .Where(link => link.RecordStatus == RecordStatus.Active && link.ProgressPercent < BusinessRules.MaxProgressPercent)
            .ToList();

        if (inProgressLinks.Count == 0)
        {
            return Array.Empty<UserCourseItemDto>();
        }

        var courseIds = inProgressLinks
            .Select(link => link.CourseId)
            .Distinct()
            .ToArray();

        var courses = await unitOfWork.CourseRepository.GetByIdsAsync(courseIds, cancellationToken);
        var courseNameById = courses.ToDictionary(course => course.Id, course => course.Name);

        var items = inProgressLinks.ToUserCourseItemDtos(courseNameById);
        return items;
    }

    public async Task<IReadOnlyList<UserCourseItemDto>> GetCompletedCoursesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var userCourses = await unitOfWork.UserCourseRepository.GetByUserIdAsync(userId, cancellationToken);

        var completedLinks = userCourses
            .Where(link => link.RecordStatus == RecordStatus.Active && link.ProgressPercent == BusinessRules.MaxProgressPercent)
            .ToList();

        if (completedLinks.Count == 0)
        {
            return Array.Empty<UserCourseItemDto>();
        }

        var courseIds = completedLinks
            .Select(link => link.CourseId)
            .Distinct()
            .ToArray();

        var courses = await unitOfWork.CourseRepository.GetByIdsAsync(courseIds, cancellationToken);
        var courseNameById = courses.ToDictionary(course => course.Id, course => course.Name);

        var items = completedLinks.ToUserCourseItemDtos(courseNameById);
        return items;
    }

    public async Task<IReadOnlyList<UserSkillItemDto>> GetSkillsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var userSkills = await unitOfWork.UserSkillRepository.GetByUserIdAsync(userId, cancellationToken);

        var activeLinks = userSkills
            .Where(link => link.RecordStatus == RecordStatus.Active)
            .ToList();

        if (activeLinks.Count == 0)
        {
            return Array.Empty<UserSkillItemDto>();
        }

        var skillIds = activeLinks
            .Select(link => link.SkillId)
            .Distinct()
            .ToArray();

        var skills = await unitOfWork.SkillRepository.GetByIdsAsync(skillIds, cancellationToken);
        var skillNameById = skills.ToDictionary(skill => skill.Id, skill => skill.Name);

        var items = activeLinks.ToUserSkillItemDtos(skillNameById);
        return items;
    }
}