using EducationPortal.DataAccess.Entities;

namespace EducationPortal.DataAccess.Abstractions;

public interface ICourseRepository
{
    Task<Course?> GetByIdAsync(int courseId, CancellationToken cancellationToken = default);
    Task<Course?> GetWithDetailsByIdAsync(int courseId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Course>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Course>> GetByIdsAsync(IReadOnlyList<int> courseIds, CancellationToken cancellationToken = default);
    Task AddAsync(Course course, CancellationToken cancellationToken = default);
    void Update(Course course);
    Task<bool> DeleteByIdAsync(int courseId, CancellationToken cancellationToken = default);
}