namespace EducationPortal.DataAccess.Entities;

public class Course
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public RecordStatus RecordStatus { get; set; } = RecordStatus.Active;

    public ICollection<CourseMaterial> CourseMaterials { get; set; } = new List<CourseMaterial>();
    public ICollection<CourseSkill> CourseSkills { get; set; } = new List<CourseSkill>();
    public ICollection<UserCourse> UserCourses { get; set; } = new List<UserCourse>();
}