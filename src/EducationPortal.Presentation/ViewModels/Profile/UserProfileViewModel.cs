namespace EducationPortal.Presentation.ViewModels.Profile;

public sealed class UserProfileViewModel
{
    public Guid UserId { get; init; }
    public string? FullName { get; init; }
    public required string Email { get; init; }
    public DateTime CreatedAt { get; init; }

    public int InProgressCoursesCount { get; init; }
    public int CompletedCoursesCount { get; init; }
    public int SkillsCount { get; init; }
}