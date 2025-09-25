using EducationPortal.BusinessLogic.DTOs.Courses;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EducationPortal.Presentation.Mappers;

public static class SelectListMapper
{
    public static List<SelectListItem> ToCourseOptions(IReadOnlyList<CourseListItemDto> courses)
        => courses
            .Select(course => new SelectListItem { Value = course.Id.ToString(), Text = course.Name })
            .OrderBy(option => option.Text)
            .ToList();
}