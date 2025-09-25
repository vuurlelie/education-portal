namespace EducationPortal.DataAccess.Entities;

public class UserCourse
{
    public Guid UserId { get; set; }
    public required ApplicationUser User { get; set; } 

    public int CourseId { get; set; }
    public required Course Course { get; set; } 

    public int StatusId { get; set; }          
    public required CourseStatus CourseStatus { get; set; } 

    public byte ProgressPercent { get; set; }
    public RecordStatus RecordStatus { get; set; } = RecordStatus.Active;
}