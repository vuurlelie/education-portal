using EducationPortal.BusinessLogic.DTOs.Materials;

namespace EducationPortal.BusinessLogic.DTOs.Courses;

public sealed class CourseMaterialItemDto
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public MaterialType Type { get; init; }
}