namespace EducationPortal.Presentation.ViewModels.Profile;

public sealed class MyCoursesViewModel
{
    public IReadOnlyList<UserCourseItemViewModel> InProgress { get; init; } = [];
    public IReadOnlyList<UserCourseItemViewModel> Completed { get; init; } = [];
    public List<CourseInProgressViewModel> InProgressDetailed { get; init; } = [];
}