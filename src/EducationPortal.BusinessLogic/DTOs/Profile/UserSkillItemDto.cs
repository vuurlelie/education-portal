namespace EducationPortal.BusinessLogic.DTOs.Profile;

public sealed class UserSkillItemDto
{
    public int SkillId { get; init; }
    public required string SkillName { get; init; }
    public int Level { get; init; }
}