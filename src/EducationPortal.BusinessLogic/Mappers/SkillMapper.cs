using EducationPortal.BusinessLogic.DTOs.Skills;
using EducationPortal.DataAccess.Entities;

namespace EducationPortal.BusinessLogic.Mappers;

public static class SkillMapper
{
    public static SkillListItemDto ToListItemDto(this Skill skill)
        => MapListItem(skill);

    public static IReadOnlyList<SkillListItemDto> ToListItemDtos(this IEnumerable<Skill> skills)
        => skills
            .Select(MapListItem)
            .ToList();

    public static SkillDetailsDto ToDetailsDto(this Skill skill)
    {
        var assignedCourseIds = MapAssignedCourseIds(skill);

        return new SkillDetailsDto
        {
            Id = skill.Id,
            Name = skill.Name,
            Description = skill.Description,
            AssignedCourseIds = assignedCourseIds
        };
    }

    public static Skill ToEntity(this SkillCreateDto dto)
    {
        return new Skill
        {
            Name = dto.Name,
            Description = dto.Description
        };
    }

    public static void ApplyChanges(this SkillEditDto changes, Skill entity)
    {
        entity.Name = changes.Name;
        entity.Description = changes.Description;
    }

    private static SkillListItemDto MapListItem(Skill skill)
        => new SkillListItemDto
        {
            Id = skill.Id,
            Name = skill.Name,
            Description = skill.Description
        };

    private static List<int> MapAssignedCourseIds(Skill skill)
        => skill.CourseSkills
            .Where(link => link.RecordStatus == RecordStatus.Active)
            .Select(link => link.CourseId)
            .Distinct()
            .OrderBy(id => id)
            .ToList();
}