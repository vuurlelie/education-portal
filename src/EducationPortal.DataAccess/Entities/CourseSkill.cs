namespace EducationPortal.DataAccess.Entities;

public class CourseSkill
{
    public int CourseId { get; set; }
    public Course? Course { get; set; } 

    public int SkillId { get; set; }
    public Skill? Skill { get; set; }

    public RecordStatus RecordStatus { get; set; } = RecordStatus.Active;
}