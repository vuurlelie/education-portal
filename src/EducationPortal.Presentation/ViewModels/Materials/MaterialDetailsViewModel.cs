namespace EducationPortal.Presentation.ViewModels.Materials;

public sealed class MaterialDetailsViewModel
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Type { get; init; } = string.Empty;

    public int? DurationSeconds { get; init; }
    public int? HeightPx { get; init; }
    public int? WidthPx { get; init; }

    public string? Authors { get; init; }
    public int? Pages { get; init; }
    public int? FormatId { get; init; }
    public string? FormatName { get; init; }
    public int? PublicationYear { get; init; }

    public DateOnly? PublishedAt { get; init; }
    public string? SourceUrl { get; init; }
}