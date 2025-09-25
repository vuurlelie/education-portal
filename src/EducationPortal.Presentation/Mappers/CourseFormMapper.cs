using EducationPortal.BusinessLogic.DTOs.Courses;
using EducationPortal.BusinessLogic.DTOs.Materials;
using EducationPortal.BusinessLogic.DTOs.Skills;
using EducationPortal.Presentation.ViewModels.Courses;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EducationPortal.Presentation.Mappers;

public static class CourseFormMapper
{
    public static CourseFormViewModel ToCreateForm(
        IReadOnlyList<MaterialListItemDto> materialDtos,
        IReadOnlyList<SkillListItemDto> skillDtos)
    {
        return new CourseFormViewModel
        {
            MaterialOptions = ToMaterialOptions(materialDtos),
            SkillOptions = ToSkillOptions(skillDtos),
            SelectedMaterialIds = [],
            SelectedSkillIds = []
        };
    }

    public static CourseFormViewModel ToEditForm(
        CourseDetailsDto courseDetailsDto,
        IReadOnlyList<MaterialListItemDto> materialDtos,
        IReadOnlyList<SkillListItemDto> skillDtos)
    {
        var selectedMaterialIds = courseDetailsDto.Materials
            .Select(material => material.Id)
            .Distinct()
            .ToArray();

        var selectedSkillIds = courseDetailsDto.Skills
            .Select(skill => skill.Id)
            .Distinct()
            .ToArray();

        return new CourseFormViewModel
        {
            Id = courseDetailsDto.Id,
            Name = courseDetailsDto.Name,
            Description = courseDetailsDto.Description,
            SelectedMaterialIds = selectedMaterialIds,
            SelectedSkillIds = selectedSkillIds,
            MaterialOptions = ToMaterialOptions(materialDtos),
            SkillOptions = ToSkillOptions(skillDtos)
        };
    }

    private static List<SelectListItem> ToMaterialOptions(IReadOnlyList<MaterialListItemDto> materials)
    {
        return materials
            .Select(material => new SelectListItem
            {
                Value = material.Id.ToString(),
                Text = material.Title
            })
            .OrderBy(option => option.Text)
            .ToList();
    }

    private static List<SelectListItem> ToSkillOptions(IReadOnlyList<SkillListItemDto> skills)
    {
        return skills
            .Select(skill => new SelectListItem
            {
                Value = skill.Id.ToString(),
                Text = skill.Name
            })
            .OrderBy(option => option.Text)
            .ToList();
    }
}