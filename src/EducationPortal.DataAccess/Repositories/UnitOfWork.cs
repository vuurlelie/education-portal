using EducationPortal.DataAccess.Abstractions;

namespace EducationPortal.DataAccess.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly IAppDbContext _databaseContext;

    private ICourseRepository? _courseRepository;
    private IMaterialRepository? _materialRepository;
    private ISkillRepository? _skillRepository;
    private IUserRepository? _userRepository;
    private IUserCourseRepository? _userCourseRepository;
    private IUserMaterialRepository? _userMaterialRepository;
    private IUserSkillRepository? _userSkillRepository;

    public UnitOfWork(IAppDbContext databaseContext)
    {
        _databaseContext = databaseContext;
    }

    public ICourseRepository CourseRepository
        => _courseRepository ??= new CourseRepository(_databaseContext);

    public IMaterialRepository MaterialRepository
        => _materialRepository ??= new MaterialRepository(_databaseContext);

    public ISkillRepository SkillRepository
        => _skillRepository ??= new SkillRepository(_databaseContext);

    public IUserRepository UserRepository
        => _userRepository ??= new UserRepository(_databaseContext);

    public IUserCourseRepository UserCourseRepository
        => _userCourseRepository ??= new UserCourseRepository(_databaseContext);

    public IUserMaterialRepository UserMaterialRepository
        => _userMaterialRepository ??= new UserMaterialRepository(_databaseContext);

    public IUserSkillRepository UserSkillRepository
        => _userSkillRepository ??= new UserSkillRepository(_databaseContext);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _databaseContext.SaveChangesAsync(cancellationToken);
}