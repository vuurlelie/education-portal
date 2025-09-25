using EducationPortal.BusinessLogic.DTOs.Courses;

namespace EducationPortal.BusinessLogic.Abstractions;

public interface ICourseService
{
    Task<IReadOnlyList<CourseListItemDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<CourseDetailsDto?> GetDetailsAsync(int courseId, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(CourseCreateDto newCourse, CancellationToken cancellationToken = default);
    Task UpdateAsync(int courseId, CourseEditDto changes, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int courseId, CancellationToken cancellationToken = default);
    Task UpdateCourseMaterialsAsync(int courseId, IReadOnlyCollection<int> materialIds, CancellationToken cancellationToken = default);
    Task UpdateCourseSkillsAsync(int courseId, IReadOnlyCollection<int> skillIds, CancellationToken cancellationToken = default);
}