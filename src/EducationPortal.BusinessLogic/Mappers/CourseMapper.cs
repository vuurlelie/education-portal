using EducationPortal.BusinessLogic.DTOs.Courses;
using EducationPortal.BusinessLogic.DTOs.Materials;
using EducationPortal.DataAccess.Entities;

namespace EducationPortal.BusinessLogic.Mappers;

public static class CourseMapper
{
    public static CourseListItemDto ToListItemDto(this Course course)
        => MapListItem(course);

    public static IReadOnlyList<CourseListItemDto> ToListItemDtos(this IEnumerable<Course> courses)
        => courses
            .Select(MapListItem)
            .ToList();

    public static CourseDetailsDto ToDetailsDto(this Course course)
    {
        var materialItems = course.CourseMaterials
            .Where(link => link.RecordStatus == RecordStatus.Active)
            .Select(link => link.Material)
            .OfType<Material>()
            .Select(MapCourseMaterialItem)
            .OrderBy(material => material.Title)
            .ToList();

        var skillItems = course.CourseSkills
            .Where(link => link.RecordStatus == RecordStatus.Active)
            .Select(link => link.Skill)
            .OfType<Skill>()
            .OrderBy(skill => skill.Name)
            .Select(MapCourseSkillItem)
            .ToList();

        return new CourseDetailsDto
        {
            Id = course.Id,
            Name = course.Name,
            Description = course.Description,
            Materials = materialItems,
            Skills = skillItems
        };
    }

    public static Course ToEntity(this CourseCreateDto dto)
    {
        return new Course
        {
            Name = dto.Name,
            Description = dto.Description
        };
    }

    public static void ApplyChanges(this CourseEditDto changes, Course entity)
    {
        entity.Name = changes.Name;
        entity.Description = changes.Description;
    }

    private static CourseListItemDto MapListItem(Course course)
        => new CourseListItemDto
        {
            Id = course.Id,
            Name = course.Name
        };

    private static CourseMaterialItemDto MapCourseMaterialItem(Material material)
        => material switch
        {
            VideoMaterial video => new CourseMaterialItemDto
            {
                Id = video.Id,
                Title = video.Title,
                Type = MaterialType.Video
            },
            BookMaterial book => new CourseMaterialItemDto
            {
                Id = book.Id,
                Title = book.Title,
                Type = MaterialType.Book
            },
            ArticleMaterial article => new CourseMaterialItemDto
            {
                Id = article.Id,
                Title = article.Title,
                Type = MaterialType.Article
            },
            _ => new CourseMaterialItemDto
            {
                Id = material.Id,
                Title = material.Title,
                Type = MaterialType.Unknown
            }
        };

    private static CourseSkillItemDto MapCourseSkillItem(Skill skill)
        => new CourseSkillItemDto
        {
            Id = skill.Id,
            Name = skill.Name
        };
}