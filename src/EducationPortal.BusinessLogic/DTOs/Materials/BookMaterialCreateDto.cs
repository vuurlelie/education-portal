namespace EducationPortal.BusinessLogic.DTOs.Materials;

public sealed class BookMaterialCreateDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Authors { get; set; } = string.Empty;
    public int Pages { get; set; }
    public int FormatId { get; set; }
    public int PublicationYear { get; set; }
}