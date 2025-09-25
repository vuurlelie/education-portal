namespace EducationPortal.DataAccess.Entities;

public class VideoMaterial : Material
{
    public int DurationSec { get; set; }
    public int HeightPx { get; set; }
    public int WidthPx { get; set; }
}