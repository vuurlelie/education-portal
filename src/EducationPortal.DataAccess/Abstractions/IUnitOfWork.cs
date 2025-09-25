namespace EducationPortal.DataAccess.Abstractions;

public interface IUnitOfWork
{
    ICourseRepository CourseRepository { get; }
    IMaterialRepository MaterialRepository { get; }
    ISkillRepository SkillRepository { get; }
    IUserRepository UserRepository { get; }
    IUserCourseRepository UserCourseRepository { get; }
    IUserMaterialRepository UserMaterialRepository { get; }
    IUserSkillRepository UserSkillRepository { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}