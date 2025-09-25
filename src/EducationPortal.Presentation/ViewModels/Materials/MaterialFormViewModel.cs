using Microsoft.AspNetCore.Mvc.Rendering;

namespace EducationPortal.Presentation.ViewModels.Materials;

public sealed class MaterialFormViewModel
{
    public int? Id { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    public string Type { get; set; } = "Video";

    public int? DurationSeconds { get; set; }
    public int? HeightPx { get; set; }
    public int? WidthPx { get; set; }

    public string? Authors { get; set; }
    public int? Pages { get; set; }
    public int? FormatId { get; set; }
    public int? PublicationYear { get; set; }

    public DateOnly? PublishedAt { get; set; }
    public string? SourceUrl { get; set; }

    public IReadOnlyList<SelectListItem> FormatOptions { get; set; } = [];
    public IReadOnlyList<SelectListItem> CourseOptions { get; set; } = [];
    public int[] SelectedCourseIds { get; set; } = [];
}