using EducationPortal.DataAccess.Abstractions;
using EducationPortal.DataAccess.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EducationPortal.DataAccess;

public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Skill> Skills => Set<Skill>();

    public DbSet<Material> Materials => Set<Material>();
    public DbSet<VideoMaterial> VideoMaterials => Set<VideoMaterial>();
    public DbSet<BookMaterial> BookMaterials => Set<BookMaterial>();
    public DbSet<ArticleMaterial> ArticleMaterials => Set<ArticleMaterial>();

    public DbSet<BookFormat> BookFormats => Set<BookFormat>();
    public DbSet<CourseStatus> CourseStatuses => Set<CourseStatus>();

    public DbSet<CourseMaterial> CourseMaterials => Set<CourseMaterial>();
    public DbSet<CourseSkill> CourseSkills => Set<CourseSkill>();
    public DbSet<UserCourse> UserCourses => Set<UserCourse>();
    public DbSet<UserMaterial> UserMaterials => Set<UserMaterial>();
    public DbSet<UserSkill> UserSkills => Set<UserSkill>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ApplicationUser>(applicationUser =>
        {
            applicationUser.Property(user => user.FullName).HasMaxLength(128);
            applicationUser.Property(user => user.CreatedAt).HasColumnType("datetime2").IsRequired();
        });

        modelBuilder.Entity<Material>().UseTptMappingStrategy();

        modelBuilder.Entity<Material>(material =>
        {
            material.ToTable("Materials");
            material.Property(property => property.Title).HasMaxLength(200).IsRequired();
            material.Property(property => property.Description).HasMaxLength(1000);
            material.Property(property => property.RecordStatus).HasConversion<byte>();
            material.HasQueryFilter(filter => filter.RecordStatus == RecordStatus.Active);
        });

        modelBuilder.Entity<VideoMaterial>(video =>
        {
            video.ToTable("VideoMaterials");
            video.Property(property => property.DurationSec).IsRequired();
            video.Property(property => property.HeightPx).IsRequired();
            video.Property(property => property.WidthPx).IsRequired();

            video.ToTable(table =>
            {
                table.HasCheckConstraint("CK_VideoMaterial_Duration_Pos", "[DurationSec] > 0");
                table.HasCheckConstraint("CK_VideoMaterial_Height_Pos", "[HeightPx] > 0");
                table.HasCheckConstraint("CK_VideoMaterial_Width_Pos", "[WidthPx]  > 0");
            });
        });

        modelBuilder.Entity<BookMaterial>(book =>
        {
            book.ToTable("BookMaterials");
            book.Property(property => property.Authors).HasMaxLength(300).IsRequired();
            book.Property(property => property.Pages).IsRequired();
            book.Property(property => property.PublicationYear).IsRequired();

            book.HasOne(property => property.Format)
                .WithMany(bookFormat => bookFormat.Books)
                .HasForeignKey(property => property.FormatId)
                .IsRequired();

            book.Navigation(property => property.Format).AutoInclude();

            book.ToTable(table =>
            {
                table.HasCheckConstraint("CK_BookMaterial_Pages_Pos", "[Pages] > 0");
                table.HasCheckConstraint("CK_BookMaterial_Year_Pos", "[PublicationYear] > 0");
            });
        });

        modelBuilder.Entity<ArticleMaterial>(article =>
        {
            article.ToTable("ArticleMaterials");
            article.Property(property => property.SourceUrl).HasMaxLength(500).IsRequired();
            article.Property(property => property.PublishedAt).HasColumnType("date");
        });

        modelBuilder.Entity<BookFormat>(bookFormat =>
        {
            bookFormat.ToTable("BookFormats");
            bookFormat.Property(property => property.Name).HasMaxLength(50).IsRequired();
            bookFormat.HasIndex(property => property.Name).IsUnique();
        });

        modelBuilder.Entity<CourseStatus>(courseStatus =>
        {
            courseStatus.ToTable("CourseStatuses");
            courseStatus.Property(property => property.Name).HasMaxLength(50).IsRequired();
            courseStatus.HasIndex(property => property.Name).IsUnique();
        });

        modelBuilder.Entity<Course>(course =>
        {
            course.ToTable("Courses");
            course.Property(property => property.Name).HasMaxLength(200).IsRequired();
            course.Property(property => property.Description).HasMaxLength(1000);
            course.Property(property => property.RecordStatus).HasConversion<byte>();
            course.HasQueryFilter(filter => filter.RecordStatus == RecordStatus.Active);
        });

        modelBuilder.Entity<Skill>(skill =>
        {
            skill.ToTable("Skills");
            skill.Property(property => property.Name).HasMaxLength(100).IsRequired();
            skill.Property(property => property.Description).HasMaxLength(500);
            skill.Property(property => property.RecordStatus).HasConversion<byte>();
            skill.HasQueryFilter(filter => filter.RecordStatus == RecordStatus.Active);
            skill.HasIndex(property => property.Name).IsUnique().HasFilter("[RecordStatus] = 1");
        });

        modelBuilder.Entity<CourseMaterial>(courseMaterial =>
        {
            courseMaterial.ToTable("CourseMaterials");
            courseMaterial.HasKey(key => new { key.CourseId, key.MaterialId });
            courseMaterial.Property(property => property.RecordStatus).HasConversion<byte>();
            courseMaterial.HasQueryFilter(filter => filter.RecordStatus == RecordStatus.Active);

            courseMaterial.HasOne(property => property.Course)
                          .WithMany(course => course.CourseMaterials)
                          .HasForeignKey(property => property.CourseId)
                          .OnDelete(DeleteBehavior.Restrict);

            courseMaterial.HasOne(property => property.Material)
                          .WithMany(material => material.CourseMaterials)
                          .HasForeignKey(property => property.MaterialId)
                          .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CourseSkill>(courseSkill =>
        {
            courseSkill.ToTable("CourseSkills");
            courseSkill.HasKey(key => new { key.CourseId, key.SkillId });
            courseSkill.Property(property => property.RecordStatus).HasConversion<byte>();
            courseSkill.HasQueryFilter(filter => filter.RecordStatus == RecordStatus.Active);

            courseSkill.HasOne(property => property.Course)
                       .WithMany(course => course.CourseSkills)
                       .HasForeignKey(property => property.CourseId)
                       .OnDelete(DeleteBehavior.Restrict);

            courseSkill.HasOne(property => property.Skill)
                       .WithMany(skill => skill.CourseSkills)
                       .HasForeignKey(property => property.SkillId)
                       .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<UserMaterial>(userMaterial =>
        {
            userMaterial.ToTable("UserMaterials");
            userMaterial.HasKey(key => new { key.UserId, key.MaterialId });
            userMaterial.Property(property => property.RecordStatus).HasConversion<byte>();
            userMaterial.HasQueryFilter(filter => filter.RecordStatus == RecordStatus.Active);

            userMaterial.HasOne(property => property.User)
                        .WithMany()
                        .HasForeignKey(property => property.UserId)
                        .OnDelete(DeleteBehavior.Restrict);

            userMaterial.HasOne(property => property.Material)
                        .WithMany(material => material.UserMaterials)
                        .HasForeignKey(property => property.MaterialId)
                        .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<UserSkill>(userSkill =>
        {
            userSkill.ToTable("UserSkills");
            userSkill.HasKey(key => new { key.UserId, key.SkillId });
            userSkill.Property(property => property.Level).IsRequired();
            userSkill.Property(property => property.RecordStatus).HasConversion<byte>();
            userSkill.HasQueryFilter(filter => filter.RecordStatus == RecordStatus.Active);

            userSkill.ToTable(table => table.HasCheckConstraint("CK_UserSkills_Level_Min1", "[Level] >= 1"));

            userSkill.HasOne(property => property.User)
                     .WithMany()
                     .HasForeignKey(property => property.UserId)
                     .OnDelete(DeleteBehavior.Restrict);

            userSkill.HasOne(property => property.Skill)
                     .WithMany(skill => skill.UserSkills)
                     .HasForeignKey(property => property.SkillId)
                     .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<UserCourse>(userCourse =>
        {
            userCourse.ToTable("UserCourses");
            userCourse.HasKey(key => new { key.UserId, key.CourseId });

            userCourse.Property(property => property.ProgressPercent)
                      .IsRequired()
                      .HasDefaultValue((byte)0);

            userCourse.ToTable(table =>
                table.HasCheckConstraint("CK_UserCourses_Progress_0_100", "[ProgressPercent] BETWEEN 0 AND 100"));

            userCourse.Property(property => property.StatusId).IsRequired();

            userCourse.Property(property => property.RecordStatus).HasConversion<byte>();
            userCourse.HasQueryFilter(filter => filter.RecordStatus == RecordStatus.Active);

            userCourse.HasOne(property => property.User)
                      .WithMany()
                      .HasForeignKey(property => property.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

            userCourse.HasOne(property => property.Course)
                      .WithMany(course => course.UserCourses)
                      .HasForeignKey(property => property.CourseId)
                      .OnDelete(DeleteBehavior.Restrict);

            userCourse.HasOne(property => property.CourseStatus)
                      .WithMany(status => status.UserCourses)
                      .HasForeignKey(property => property.StatusId)
                      .OnDelete(DeleteBehavior.Restrict)
                      .IsRequired();
        });
    }
}