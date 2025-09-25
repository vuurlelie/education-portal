namespace EducationPortal.DataAccess.Entities;

public class Skill
{
    public int Id { get; set; }
    public required string Name { get; set; } 
    public string? Description { get; set; }
    public RecordStatus RecordStatus { get; set; } = RecordStatus.Active;

    public ICollection<CourseSkill> CourseSkills { get; set; } = new List<CourseSkill>();
    public ICollection<UserSkill> UserSkills { get; set; } = new List<UserSkill>();
}