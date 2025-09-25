using EducationPortal.BusinessLogic.DTOs.Courses;
using EducationPortal.Presentation.ViewModels.Courses;

namespace EducationPortal.Presentation.Mappers;

public static class CourseViewMapper
{
    public static IReadOnlyList<CourseListItemViewModel> ToListItems(
        IReadOnlyList<CourseListItemDto> courseDtos,
        IReadOnlyDictionary<int, bool>? canStartByCourseId = null)
    {
        var items = courseDtos
            .Select(courseDto => new CourseListItemViewModel
            {
                Id = courseDto.Id,
                Name = courseDto.Name,
                CanStart = canStartByCourseId != null
                           && canStartByCourseId.TryGetValue(courseDto.Id, out var canStart)
                           && canStart
            })
            .ToList();

        return items;
    }

    public static CourseDetailsViewModel ToDetails(CourseDetailsDto detailsDto, bool canStart)
    {
        var materialSummaries = detailsDto.Materials
            .Select(materialDto => new CourseMaterialSummaryViewModel
            {
                Id = materialDto.Id,
                Title = materialDto.Title,
                Type = materialDto.Type
            })
            .ToList();

        var skillSummaries = detailsDto.Skills
            .Select(skillDto => new CourseSkillSummaryViewModel
            {
                Id = skillDto.Id,
                Name = skillDto.Name
            })
            .ToList();

        return new CourseDetailsViewModel
        {
            Id = detailsDto.Id,
            Name = detailsDto.Name,
            Description = detailsDto.Description,
            Materials = materialSummaries,
            Skills = skillSummaries,
            CanStart = canStart
        };
    }
}