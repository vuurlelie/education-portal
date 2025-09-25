using Microsoft.AspNetCore.Mvc.Rendering;

namespace EducationPortal.Presentation.ViewModels.Skills;

public sealed class SkillFormViewModel
{
    public int? Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public IReadOnlyList<SelectListItem> CourseOptions { get; set; } = [];
    public int[] SelectedCourseIds { get; set; } = [];
}