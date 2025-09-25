namespace EducationPortal.BusinessLogic.DTOs.Materials;

public sealed class ArticleMaterialEditDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateOnly? PublishedAt { get; set; }
    public string? SourceUrl { get; set; }
}