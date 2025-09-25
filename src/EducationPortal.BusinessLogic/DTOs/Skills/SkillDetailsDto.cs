namespace EducationPortal.BusinessLogic.DTOs.Skills;

public sealed class SkillDetailsDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public IReadOnlyList<int> AssignedCourseIds { get; init; } = [];
}