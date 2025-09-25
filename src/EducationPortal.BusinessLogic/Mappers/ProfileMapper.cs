using EducationPortal.BusinessLogic.DTOs.Profile;
using EducationPortal.DataAccess.Entities;

namespace EducationPortal.BusinessLogic.Mappers;

public static class ProfileMapper
{
    public static UserProfileDto ToUserProfileDto(this ApplicationUser user, int inProgressCount, int completedCount, int activeSkillsCount)
    {
        return new UserProfileDto
        {
            UserId = user.Id,
            FullName = user.FullName ?? "Not set",
            Email = user.Email ?? user.UserName ?? string.Empty,
            CreatedAt = user.CreatedAt,
            InProgressCoursesCount = inProgressCount,
            CompletedCoursesCount = completedCount,
            SkillsCount = activeSkillsCount
        };
    }

    public static IReadOnlyList<UserCourseItemDto> ToUserCourseItemDtos(this IEnumerable<UserCourse> userCourses, IDictionary<int, string> courseNameById)
    {
        var items = userCourses
            .Select(link => MapUserCourseItem(link, courseNameById))
            .OrderBy(item => item.CourseName)
            .ToList();

        return items;
    }

    public static IReadOnlyList<UserSkillItemDto> ToUserSkillItemDtos(this IEnumerable<UserSkill> userSkills, IDictionary<int, string> skillNameById)
    {
        var items = userSkills
            .Select(link => MapUserSkillItem(link, skillNameById))
            .OrderBy(item => item.SkillName)
            .ToList();

        return items;
    }

    private static UserCourseItemDto MapUserCourseItem(UserCourse link, IDictionary<int, string> courseNameById)
    {
        courseNameById.TryGetValue(link.CourseId, out var courseName);
        var resolvedName = courseName ?? "(deleted course)";

        return new UserCourseItemDto
        {
            CourseId = link.CourseId,
            CourseName = resolvedName,
            ProgressPercent = link.ProgressPercent
        };
    }

    private static UserSkillItemDto MapUserSkillItem(UserSkill link, IDictionary<int, string> skillNameById)
    {
        skillNameById.TryGetValue(link.SkillId, out var skillName);
        var resolvedName = skillName ?? "(deleted skill)";

        return new UserSkillItemDto
        {
            SkillId = link.SkillId,
            SkillName = resolvedName,
            Level = link.Level
        };
    }
}