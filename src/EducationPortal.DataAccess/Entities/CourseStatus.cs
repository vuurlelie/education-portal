namespace EducationPortal.DataAccess.Entities;

public class CourseStatus
{
    public int Id { get; set; }         

    public required string Name { get; set; }

    public ICollection<UserCourse> UserCourses { get; set; } = new List<UserCourse>();
}