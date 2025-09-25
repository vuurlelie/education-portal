namespace EducationPortal.DataAccess.Entities;

public class CourseMaterial
{
    public int CourseId { get; set; }
    public Course? Course { get; set; }

    public int MaterialId { get; set; }
    public Material? Material { get; set; }

    public RecordStatus RecordStatus { get; set; } = RecordStatus.Active;
}