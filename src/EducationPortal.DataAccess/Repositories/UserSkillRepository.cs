using EducationPortal.DataAccess.Abstractions;
using EducationPortal.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace EducationPortal.DataAccess.Repositories;

public sealed class UserSkillRepository : IUserSkillRepository
{
    private readonly IAppDbContext _databaseContext;

    public UserSkillRepository(IAppDbContext databaseContext)
    {
        _databaseContext = databaseContext;
    }

    public async Task<IReadOnlyList<UserSkill>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _databaseContext.UserSkills
            .Where(userSkill =>
                userSkill.UserId == userId &&
                userSkill.RecordStatus == RecordStatus.Active)
            .Include(userSkill => userSkill.Skill)
            .ToListAsync(cancellationToken);
    }

    public async Task<UserSkill?> GetAsync(Guid userId, int skillId, CancellationToken cancellationToken = default)
    {
        return await _databaseContext.UserSkills
            .Where(userSkill =>
                userSkill.UserId == userId &&
                userSkill.SkillId == skillId &&
                userSkill.RecordStatus == RecordStatus.Active)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task AddAsync(UserSkill userSkill, CancellationToken cancellationToken = default)
    {
        await _databaseContext.UserSkills.AddAsync(userSkill, cancellationToken);
    }

    public void Update(UserSkill userSkill)
    {
        _databaseContext.UserSkills.Update(userSkill);
    }

    public async Task<UserSkill?> GetByUserAndSkillAsync(Guid userId, int skillId, CancellationToken cancellationToken = default)
    {
        return await _databaseContext.UserSkills
            .Include(userSkill => userSkill.User)
            .Include(userSkill => userSkill.Skill)
            .SingleOrDefaultAsync(userSkill => userSkill.User.Id == userId && userSkill.Skill.Id == skillId, cancellationToken);
    }

    public Task<bool> AnyBySkillIdAsync(int skillId, CancellationToken cancellationToken = default)
    {
        return _databaseContext.UserSkills
            .AsNoTracking()
            .AnyAsync(userSkill => userSkill.SkillId == skillId && userSkill.RecordStatus == RecordStatus.Active, cancellationToken);
    }
}