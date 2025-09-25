using Microsoft.AspNetCore.Mvc.Rendering;

namespace EducationPortal.Presentation.ViewModels.Courses;

public sealed class CourseFormViewModel
{
    public int? Id { get; init; }

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public int[] SelectedMaterialIds { get; set; } = [];
    public int[] SelectedSkillIds { get; set; } = [];

    public List<SelectListItem> MaterialOptions { get; set; } = [];
    public List<SelectListItem> SkillOptions { get; set; } = [];
}