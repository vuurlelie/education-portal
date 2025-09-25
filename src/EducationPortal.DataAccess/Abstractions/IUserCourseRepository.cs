using EducationPortal.DataAccess.Entities;

namespace EducationPortal.DataAccess.Abstractions;

public interface IUserCourseRepository
{
    Task<UserCourse?> GetAsync(Guid userId, int courseId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserCourse>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(UserCourse userCourse, CancellationToken cancellationToken = default);
    void Update(UserCourse userCourse);
    Task<CourseStatus?> GetStatusByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<UserCourse?> GetByUserAndCourseAsync(Guid userId, int courseId, CancellationToken cancellationToken = default);
    Task<UserCourse?> GetByUserAndCourseWithStatusAsync(Guid userId, int courseId, CancellationToken cancellationToken = default);
    Task<bool> AnyActiveByCourseIdAsync(int courseId, CancellationToken cancellationToken = default);
}