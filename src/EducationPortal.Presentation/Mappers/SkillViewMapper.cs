using EducationPortal.BusinessLogic.DTOs.Courses;
using EducationPortal.BusinessLogic.DTOs.Skills;
using EducationPortal.Presentation.ViewModels.Skills;

namespace EducationPortal.Presentation.Mappers;

public static class SkillViewMapper
{
    public static List<SkillListItemViewModel> ToListItemViewModels(
        IReadOnlyList<SkillListItemDto> skillDtos)
        => skillDtos
            .Select(skill => new SkillListItemViewModel
            {
                Id = skill.Id,
                Name = skill.Name,
                Description = skill.Description
            })
            .ToList();

    public static SkillDetailsViewModel ToDetailsViewModel(
        SkillDetailsDto details,
        IReadOnlyDictionary<int, string> courseNameById)
    {
        var courseItems = details.AssignedCourseIds
            .Select(courseId => new SkillDetailsCourseItemViewModel
            {
                Id = courseId,
                Name = courseNameById.TryGetValue(courseId, out var name) ? name : "(deleted course)"
            })
            .OrderBy(item => item.Name)
            .ToList();

        return new SkillDetailsViewModel
        {
            Id = details.Id,
            Name = details.Name,
            Description = details.Description,
            Courses = courseItems
        };
    }

    public static SkillFormViewModel ToCreateFormViewModel(IReadOnlyList<CourseListItemDto> courseOptions)
        => new()
        {
            CourseOptions = SelectListMapper.ToCourseOptions(courseOptions)
        };

    public static SkillFormViewModel ToEditFormViewModel(
        SkillDetailsDto details,
        IReadOnlyList<CourseListItemDto> courseOptions,
        IReadOnlyCollection<int> selectedCourseIds)
        => new()
        {
            Id = details.Id,
            Name = details.Name,
            Description = details.Description,
            CourseOptions = SelectListMapper.ToCourseOptions(courseOptions),
            SelectedCourseIds = selectedCourseIds.ToArray()
        };
}