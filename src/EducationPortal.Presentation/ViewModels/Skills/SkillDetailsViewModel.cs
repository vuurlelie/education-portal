namespace EducationPortal.Presentation.ViewModels.Skills;

public sealed class SkillDetailsViewModel
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public IReadOnlyList<SkillDetailsCourseItemViewModel> Courses { get; init; } = [];
}