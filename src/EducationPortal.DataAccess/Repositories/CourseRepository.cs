using EducationPortal.DataAccess.Abstractions;
using EducationPortal.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace EducationPortal.DataAccess.Repositories;

public sealed class CourseRepository : ICourseRepository
{
    private readonly IAppDbContext _databaseContext;

    public CourseRepository(IAppDbContext databaseContext)
    {
        _databaseContext = databaseContext;
    }

    public async Task<Course?> GetByIdAsync(int courseId, CancellationToken cancellationToken = default)
    {
        return await _databaseContext.Courses
            .SingleOrDefaultAsync(course => course.Id == courseId, cancellationToken);
    }

    public async Task<Course?> GetWithDetailsByIdAsync(int courseId, CancellationToken cancellationToken = default)
    {
        return await _databaseContext.Courses
            .Include(course => course.CourseMaterials)
                .ThenInclude(link => link.Material)
            .Include(course => course.CourseSkills)
                .ThenInclude(link => link.Skill)
            .SingleOrDefaultAsync(course => course.Id == courseId, cancellationToken);
    }

    public async Task<IReadOnlyList<Course>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var courses = await _databaseContext.Courses
            .AsNoTracking()
            .OrderBy(course => course.Name)
            .ToListAsync(cancellationToken);

        return courses;
    }

    public async Task<IReadOnlyList<Course>> GetByIdsAsync(IReadOnlyList<int> courseIds, CancellationToken cancellationToken = default)
    {
        if (courseIds is null || courseIds.Count == 0)
        {
            return Array.Empty<Course>();
        }

        var courses = await _databaseContext.Courses
            .Where(course => courseIds.Contains(course.Id))
            .ToListAsync(cancellationToken);

        return courses;
    }

    public async Task AddAsync(Course course, CancellationToken cancellationToken = default)
    {
        await _databaseContext.Courses.AddAsync(course, cancellationToken);
    }

    public void Update(Course course)
    {
        _databaseContext.Courses.Update(course);
    }

    public async Task<bool> DeleteByIdAsync(int courseId, CancellationToken cancellationToken = default)
    {
        var course = await _databaseContext.Courses
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(course => course.Id == courseId, cancellationToken);

        if (course is null || course.RecordStatus == RecordStatus.Deleted)
        {
            return false;
        }

        course.RecordStatus = RecordStatus.Deleted;
        _databaseContext.Courses.Update(course);
        return true;
    }
}