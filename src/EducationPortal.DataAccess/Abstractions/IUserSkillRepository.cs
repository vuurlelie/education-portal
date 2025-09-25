using EducationPortal.DataAccess.Entities;

namespace EducationPortal.DataAccess.Abstractions;

public interface IUserSkillRepository
{
    Task<IReadOnlyList<UserSkill>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserSkill?> GetAsync(Guid userId, int skillId, CancellationToken cancellationToken = default);
    Task AddAsync(UserSkill userSkill, CancellationToken cancellationToken = default);
    void Update(UserSkill userSkill);
    Task<UserSkill?> GetByUserAndSkillAsync(Guid userId, int skillId, CancellationToken cancellationToken = default);
    Task<bool> AnyBySkillIdAsync(int skillId, CancellationToken cancellationToken = default);
}