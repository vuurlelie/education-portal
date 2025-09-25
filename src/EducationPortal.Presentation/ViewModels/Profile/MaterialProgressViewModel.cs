namespace EducationPortal.Presentation.ViewModels.Profile;

public sealed class MaterialProgressViewModel
{
    public int MaterialId { get; init; }
    public string Title { get; init; } = string.Empty;
    public bool IsCompleted { get; init; }
}