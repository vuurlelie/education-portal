namespace EducationPortal.Presentation.ViewModels.Courses;

public sealed class CourseListItemViewModel
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool CanStart { get; set; }
}