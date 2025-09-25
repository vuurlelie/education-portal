namespace EducationPortal.Presentation.ViewModels.Profile;

public sealed class UserCourseItemViewModel
{
    public int CourseId { get; init; }
    public required string CourseName { get; init; }
    public byte ProgressPercent { get; init; }
}