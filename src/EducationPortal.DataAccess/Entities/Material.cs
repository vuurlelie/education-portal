namespace EducationPortal.DataAccess.Entities;

public abstract class Material
{
    public int Id { get; set; }
    public required string Title { get; set; } 
    public string? Description { get; set; }
    public RecordStatus RecordStatus { get; set; } = RecordStatus.Active;

    public ICollection<CourseMaterial> CourseMaterials { get; set; } = new List<CourseMaterial>();
    public ICollection<UserMaterial> UserMaterials { get; set; } = new List<UserMaterial>();
}