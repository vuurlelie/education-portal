namespace EducationPortal.BusinessLogic.DTOs.Profile;

public sealed class UserCourseItemDto
{
    public int CourseId { get; init; }
    public required string CourseName { get; init; }
    public byte ProgressPercent { get; init; }
}