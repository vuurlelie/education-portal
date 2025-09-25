using EducationPortal.DataAccess.Abstractions;
using EducationPortal.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace EducationPortal.DataAccess.Repositories;

public sealed class SkillRepository : ISkillRepository
{
    private readonly IAppDbContext _databaseContext;

    public SkillRepository(IAppDbContext databaseContext)
    {
        _databaseContext = databaseContext;
    }

    public async Task<Skill?> GetByIdAsync(int skillId, CancellationToken cancellationToken = default)
    {
        return await _databaseContext.Skills
            .SingleOrDefaultAsync(skill => skill.Id == skillId, cancellationToken);
    }

    public async Task<Skill?> GetWithDetailsByIdAsync(int skillId, CancellationToken cancellationToken = default)
    {
        return await _databaseContext.Skills
            .Include(skill => skill.CourseSkills)
                .ThenInclude(link => link.Course)
            .Include(skill => skill.UserSkills)
            .SingleOrDefaultAsync(skill => skill.Id == skillId, cancellationToken);
    }

    public async Task<IReadOnlyList<Skill>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var skills = await _databaseContext.Skills
            .OrderBy(skill => skill.Name)
            .ToListAsync(cancellationToken);

        return skills;
    }

    public async Task<IReadOnlyList<Skill>> GetByIdsAsync(IReadOnlyList<int> skillIds, CancellationToken cancellationToken = default)
    {
        var skills = await _databaseContext.Skills
            .Where(skill => skillIds.Contains(skill.Id))
            .ToListAsync(cancellationToken);

        return skills;
    }

    public async Task AddAsync(Skill skill, CancellationToken cancellationToken = default)
    {
        await _databaseContext.Skills.AddAsync(skill, cancellationToken);
    }

    public void Update(Skill skill)
    {
        _databaseContext.Skills.Update(skill);
    }

    public async Task<bool> DeleteByIdAsync(int skillId, CancellationToken cancellationToken = default)
    {
        var skill = await _databaseContext.Skills
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(skill => skill.Id == skillId, cancellationToken);

        if (skill is null || skill.RecordStatus == RecordStatus.Deleted)
        {
            return false;
        }

        skill.RecordStatus = RecordStatus.Deleted;
        _databaseContext.Skills.Update(skill);

        return true;
    }
}