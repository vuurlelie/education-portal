namespace EducationPortal.Presentation.ViewModels.Profile;

public sealed class CourseInProgressViewModel
{
    public int CourseId { get; init; }
    public string CourseName { get; init; } = string.Empty;
    public int ProgressPercent { get; init; }
    public IReadOnlyList<MaterialProgressViewModel> Materials { get; init; } = [];
}