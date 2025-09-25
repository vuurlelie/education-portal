namespace EducationPortal.BusinessLogic.DTOs.Skills;

public sealed class SkillListItemDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}