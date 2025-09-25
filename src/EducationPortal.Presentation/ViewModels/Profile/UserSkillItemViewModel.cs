namespace EducationPortal.Presentation.ViewModels.Profile;

public sealed class UserSkillItemViewModel
{
    public int SkillId { get; init; }
    public required string SkillName { get; init; }
    public int Level { get; init; }
}