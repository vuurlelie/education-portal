namespace EducationPortal.Presentation.ViewModels.Materials;

public sealed class MaterialListItemViewModel
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string? FormatName { get; init; }
}