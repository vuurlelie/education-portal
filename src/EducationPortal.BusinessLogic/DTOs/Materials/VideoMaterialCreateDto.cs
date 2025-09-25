namespace EducationPortal.BusinessLogic.DTOs.Materials;

public sealed class VideoMaterialCreateDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DurationSeconds { get; set; }
    public int HeightPx { get; set; }
    public int WidthPx { get; set; }
}