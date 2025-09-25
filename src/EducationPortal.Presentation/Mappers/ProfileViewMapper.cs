using EducationPortal.BusinessLogic.DTOs.Courses;
using EducationPortal.BusinessLogic.DTOs.Profile;
using EducationPortal.Presentation.ViewModels.Profile;

namespace EducationPortal.Presentation.Mappers;

public static class ProfileViewMapper
{
    public static UserProfileViewModel ToViewModel(this UserProfileDto profile)
        => new()
        {
            UserId = profile.UserId,
            FullName = profile.FullName,
            Email = profile.Email,
            CreatedAt = profile.CreatedAt,
            InProgressCoursesCount = profile.InProgressCoursesCount,
            CompletedCoursesCount = profile.CompletedCoursesCount,
            SkillsCount = profile.SkillsCount
        };

    public static List<UserCourseItemViewModel> ToUserCourseItemViewModels(this IReadOnlyList<UserCourseItemDto> courseItems)
        => courseItems
            .Select(item => new UserCourseItemViewModel
            {
                CourseId = item.CourseId,
                CourseName = item.CourseName,
                ProgressPercent = item.ProgressPercent
            })
            .ToList();

    public static CourseInProgressViewModel ToCourseInProgressViewModel(
        this UserCourseItemDto courseItem,
        IReadOnlyList<CourseMaterialItemDto> courseMaterialItems,
        ISet<int> completedMaterialIds)
    {
        var materialViewModels = courseMaterialItems
            .OrderBy(material => material.Title)
            .Select(material => new MaterialProgressViewModel
            {
                MaterialId = material.Id,
                Title = material.Title,
                IsCompleted = completedMaterialIds.Contains(material.Id)
            })
            .ToList();

        return new CourseInProgressViewModel
        {
            CourseId = courseItem.CourseId,
            CourseName = courseItem.CourseName,
            ProgressPercent = courseItem.ProgressPercent,
            Materials = materialViewModels
        };
    }
}