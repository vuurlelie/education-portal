namespace EducationPortal.BusinessLogic.DTOs.Materials;

public sealed class MaterialListItemDto
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public MaterialType Type { get; init; } 

    public int? FormatId { get; init; }
    public string? FormatName { get; init; }
}