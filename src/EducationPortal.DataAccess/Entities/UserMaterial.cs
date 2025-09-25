namespace EducationPortal.DataAccess.Entities;

public class UserMaterial
{
    public Guid UserId { get; set; }
    public required ApplicationUser User { get; set; } 

    public int MaterialId { get; set; }
    public required Material Material { get; set; }

    public RecordStatus RecordStatus { get; set; } = RecordStatus.Active;
}