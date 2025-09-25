namespace EducationPortal.Presentation.ViewModels.Courses;

public sealed class CourseDetailsViewModel
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool CanStart { get; set; }
    public IReadOnlyList<CourseMaterialSummaryViewModel> Materials { get; init; } = [];
    public IReadOnlyList<CourseSkillSummaryViewModel> Skills { get; init; } = [];
}