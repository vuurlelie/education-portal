using EducationPortal.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace EducationPortal.DataAccess.Abstractions;

public interface IAppDbContext
{
    DbSet<ApplicationUser> Users { get; }
    DbSet<Course> Courses { get; }
    DbSet<Material> Materials { get; }
    DbSet<VideoMaterial> VideoMaterials { get; }
    DbSet<BookMaterial> BookMaterials { get; }
    DbSet<ArticleMaterial> ArticleMaterials { get; }
    DbSet<Skill> Skills { get; }
    DbSet<BookFormat> BookFormats { get; }
    DbSet<CourseStatus> CourseStatuses { get; }

    DbSet<CourseMaterial> CourseMaterials { get; }
    DbSet<CourseSkill> CourseSkills { get; }
    DbSet<UserCourse> UserCourses { get; }
    DbSet<UserMaterial> UserMaterials { get; }
    DbSet<UserSkill> UserSkills { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}