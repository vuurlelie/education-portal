using EducationPortal.DataAccess.Abstractions;
using EducationPortal.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace EducationPortal.DataAccess.Repositories;

public sealed class UserCourseRepository : IUserCourseRepository
{
    private readonly IAppDbContext _databaseContext;

    public UserCourseRepository(IAppDbContext databaseContext)
    {
        _databaseContext = databaseContext;
    }

    public async Task<UserCourse?> GetAsync(Guid userId, int courseId, CancellationToken cancellationToken = default)
    {
        return await _databaseContext.UserCourses
            .Where(userCourse =>
                userCourse.UserId == userId &&
                userCourse.CourseId == courseId &&
                userCourse.RecordStatus == RecordStatus.Active)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UserCourse>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _databaseContext.UserCourses
            .Where(userCourse =>
                userCourse.UserId == userId &&
                userCourse.RecordStatus == RecordStatus.Active)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(UserCourse userCourse, CancellationToken cancellationToken = default)
    {
        await _databaseContext.UserCourses.AddAsync(userCourse, cancellationToken);
    }

    public void Update(UserCourse userCourse)
    {
        _databaseContext.UserCourses.Update(userCourse);
    }

    public async Task<CourseStatus?> GetStatusByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _databaseContext.CourseStatuses
            .SingleOrDefaultAsync(status => status.Name == name, cancellationToken);
    }

    public async Task<UserCourse?> GetByUserAndCourseAsync(Guid userId, int courseId, CancellationToken cancellationToken = default)
    {
        return await _databaseContext.UserCourses
            .Include(userCourse => userCourse.User)
            .Include(userCourse => userCourse.Course)
            .Include(userCourse => userCourse.CourseStatus)
            .SingleOrDefaultAsync(userCourse => userCourse.User.Id == userId && userCourse.Course.Id == courseId, cancellationToken);
    }

    public async Task<UserCourse?> GetByUserAndCourseWithStatusAsync(Guid userId, int courseId, CancellationToken cancellationToken = default)
    {
        return await _databaseContext.UserCourses
            .Include(userCourse => userCourse.CourseStatus)
            .SingleOrDefaultAsync(userCourse => userCourse.UserId == userId && userCourse.CourseId == courseId, cancellationToken);
    }

    public Task<bool> AnyActiveByCourseIdAsync(int courseId, CancellationToken cancellationToken = default)
    {
        return _databaseContext.UserCourses
            .AsNoTracking()
            .AnyAsync(userCourse => userCourse.CourseId == courseId && userCourse.RecordStatus == RecordStatus.Active, cancellationToken);
    }
}