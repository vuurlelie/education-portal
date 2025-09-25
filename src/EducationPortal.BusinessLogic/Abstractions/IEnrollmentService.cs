using EducationPortal.DataAccess.Enums;

namespace EducationPortal.BusinessLogic.Abstractions;

public interface IEnrollmentService
{
    Task<CourseEnrollmentState> GetUserCourseStatusAsync(Guid userId, int courseId, CancellationToken cancellationToken = default);
    Task EnrollAsync(Guid userId, int courseId, CancellationToken cancellationToken = default);
    Task MarkMaterialCompleteAsync(Guid userId, int materialId, CancellationToken cancellationToken = default);
    Task CompleteCourseAsync(Guid userId, int courseId, CancellationToken cancellationToken = default);
    Task<HashSet<int>> GetUserCompletedMaterialIdsAsync(Guid userId, CancellationToken cancellationToken = default);
}