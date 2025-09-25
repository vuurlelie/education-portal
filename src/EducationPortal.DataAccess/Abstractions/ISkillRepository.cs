using EducationPortal.DataAccess.Entities;

namespace EducationPortal.DataAccess.Abstractions;

public interface ISkillRepository
{
    Task<Skill?> GetByIdAsync(int skillId, CancellationToken cancellationToken = default);
    Task<Skill?> GetWithDetailsByIdAsync(int skillId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Skill>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Skill>> GetByIdsAsync(IReadOnlyList<int> skillIds, CancellationToken cancellationToken = default);
    Task AddAsync(Skill skill, CancellationToken cancellationToken = default);
    void Update(Skill skill);
    Task<bool> DeleteByIdAsync(int skillId, CancellationToken cancellationToken = default);
}