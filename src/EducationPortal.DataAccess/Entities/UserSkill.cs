namespace EducationPortal.DataAccess.Entities;

public class UserSkill
{
    public Guid UserId { get; set; }
    public required ApplicationUser User { get; set; } 

    public int SkillId { get; set; }
    public required Skill Skill { get; set; } 

    public int Level { get; set; }
    public RecordStatus RecordStatus { get; set; } = RecordStatus.Active;
}