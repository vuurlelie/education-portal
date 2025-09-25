namespace EducationPortal.BusinessLogic.DTOs.Courses;

public sealed class CourseCreateDto
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}