using EducationPortal.BusinessLogic.DTOs.Materials;

namespace EducationPortal.Presentation.ViewModels.Courses;
public sealed class CourseMaterialSummaryViewModel
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public MaterialType Type { get; init; }
}