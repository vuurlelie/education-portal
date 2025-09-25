namespace EducationPortal.BusinessLogic.DTOs.Courses;

public sealed class CourseDetailsDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public IReadOnlyList<CourseMaterialItemDto> Materials { get; init; } = [];
    public IReadOnlyList<CourseSkillItemDto> Skills { get; init; } = [];
}