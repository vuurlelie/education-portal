using EducationPortal.DataAccess;
using EducationPortal.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace EducationPortal.Presentation.Infrastructure.Extensions;

public static class SeedExtensions
{
    public static async Task ResetDemoDomainAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var databaseContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await databaseContext.Database.MigrateAsync();

        using var transaction = await databaseContext.Database.BeginTransactionAsync();

        await databaseContext.Database.ExecuteSqlRawAsync("DELETE FROM [CourseMaterials];");
        await databaseContext.Database.ExecuteSqlRawAsync("DELETE FROM [CourseSkills];");
        await databaseContext.Database.ExecuteSqlRawAsync("DELETE FROM [UserMaterials];");
        await databaseContext.Database.ExecuteSqlRawAsync("DELETE FROM [UserCourses];");
        await databaseContext.Database.ExecuteSqlRawAsync("DELETE FROM [UserSkills];");

        await databaseContext.Database.ExecuteSqlRawAsync("DELETE FROM [ArticleMaterials];");
        await databaseContext.Database.ExecuteSqlRawAsync("DELETE FROM [BookMaterials];");
        await databaseContext.Database.ExecuteSqlRawAsync("DELETE FROM [VideoMaterials];");

        await databaseContext.Database.ExecuteSqlRawAsync("DELETE FROM [Materials];");

        await databaseContext.Database.ExecuteSqlRawAsync("DELETE FROM [Courses];");
        await databaseContext.Database.ExecuteSqlRawAsync("DELETE FROM [Skills];");

        await databaseContext.Database.ExecuteSqlRawAsync("DELETE FROM [BookFormats];");
        await databaseContext.Database.ExecuteSqlRawAsync("DELETE FROM [CourseStatuses];");

        var reseedSql = @"
            IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE [object_id] = OBJECT_ID(N'[dbo].[Materials]'))
                DBCC CHECKIDENT ('[dbo].[Materials]', RESEED, 0);
            IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE [object_id] = OBJECT_ID(N'[dbo].[Courses]'))
                DBCC CHECKIDENT ('[dbo].[Courses]', RESEED, 0);
            IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE [object_id] = OBJECT_ID(N'[dbo].[Skills]'))
                DBCC CHECKIDENT ('[dbo].[Skills]', RESEED, 0);
            IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE [object_id] = OBJECT_ID(N'[dbo].[BookFormats]'))
                DBCC CHECKIDENT ('[dbo].[BookFormats]', RESEED, 0);
            IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE [object_id] = OBJECT_ID(N'[dbo].[CourseStatuses]'))
                DBCC CHECKIDENT ('[dbo].[CourseStatuses]', RESEED, 0);";
        await databaseContext.Database.ExecuteSqlRawAsync(reseedSql);

        await transaction.CommitAsync();
    }


    public static async Task SeedLookupsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var databaseContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await databaseContext.Database.MigrateAsync();

        var requiredFormats = new[] { "PDF", "EPUB", "MOBI" };
        var existingFormatNames = await databaseContext.BookFormats
            .Select(format => format.Name)
            .ToListAsync();

        var missingFormats = requiredFormats
            .Except(existingFormatNames, StringComparer.OrdinalIgnoreCase)
            .Select(name => new BookFormat { Name = name })
            .ToList();

        if (missingFormats.Count > 0)
        {
            databaseContext.BookFormats.AddRange(missingFormats);
            await databaseContext.SaveChangesAsync();
        }

        var requiredStatuses = new[] { "InProgress", "Completed" };
        var existingStatusNames = await databaseContext.CourseStatuses
            .Select(status => status.Name)
            .ToListAsync();

        var missingStatuses = requiredStatuses
            .Except(existingStatusNames, StringComparer.OrdinalIgnoreCase)
            .Select(name => new CourseStatus { Name = name })
            .ToList();

        if (missingStatuses.Count > 0)
        {
            databaseContext.CourseStatuses.AddRange(missingStatuses);
            await databaseContext.SaveChangesAsync();
        }
    }

    public static async Task SeedDemoDomainAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var databaseContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await databaseContext.Database.MigrateAsync();

        if (!await databaseContext.Skills.AnyAsync())
        {
            databaseContext.Skills.AddRange(
                new Skill { Name = "C# Fundamentals" },
                new Skill { Name = "OOP in C#" },
                new Skill { Name = "ASP.NET Core" },
                new Skill { Name = "Entity Framework Core" },
                new Skill { Name = "LINQ" },
                new Skill { Name = "SQL Basics" }
            );
            await databaseContext.SaveChangesAsync();
        }

        if (!await databaseContext.Materials.AnyAsync())
        {
            var pdfFormatId = await databaseContext.BookFormats
                .Where(format => format.Name == "PDF")
                .Select(format => format.Id)
                .SingleAsync();

            var epubFormatId = await databaseContext.BookFormats
                .Where(format => format.Name == "EPUB")
                .Select(format => format.Id)
                .SingleAsync();

            var videoIntroToCSharp = new VideoMaterial
            {
                Title = "Intro to C# and .NET",
                Description = "Types, control flow, and the .NET runtime basics.",
                DurationSec = 600,
                WidthPx = 1920,
                HeightPx = 1080
            };

            var videoAspNetMvcOverview = new VideoMaterial
            {
                Title = "ASP.NET Core MVC in 60 minutes",
                Description = "Controllers, views, routing, model binding.",
                DurationSec = 3600,
                WidthPx = 1920,
                HeightPx = 1080
            };

            var videoEfCoreRelationships = new VideoMaterial
            {
                Title = "EF Core Relationships Deep Dive",
                Description = "One-to-many, many-to-many, TPT; navigations and conventions.",
                DurationSec = 2700,
                WidthPx = 1920,
                HeightPx = 1080
            };

            var bookProCSharp = new BookMaterial
            {
                Title = "Pro C# and .NET",
                Description = "C# 12 and modern .NET overview.",
                Authors = "Troelsen; Japikse",
                Pages = 1200,
                FormatId = pdfFormatId,
                PublicationYear = 2024
            };

            var bookEfCoreInAction = new BookMaterial
            {
                Title = "EF Core in Action",
                Description = "A practical approach to Entity Framework Core.",
                Authors = "Jon P Smith",
                Pages = 450,
                FormatId = epubFormatId,
                PublicationYear = 2021
            };

            var articleDependencyInjection = new ArticleMaterial
            {
                Title = "What is Dependency Injection in ASP.NET Core?",
                Description = "Conceptual overview and DI patterns.",
                PublishedAt = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                SourceUrl = "https://learn.microsoft.com/aspnet/core/fundamentals/dependency-injection"
            };

            var articleLinqFundamentals = new ArticleMaterial
            {
                Title = "LINQ fundamentals",
                Description = "Query vs method syntax and deferred execution.",
                PublishedAt = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                SourceUrl = "https://learn.microsoft.com/dotnet/csharp/linq/"
            };

            databaseContext.VideoMaterials.AddRange(
                videoIntroToCSharp,
                videoAspNetMvcOverview,
                videoEfCoreRelationships
            );

            databaseContext.BookMaterials.AddRange(
                bookProCSharp,
                bookEfCoreInAction
            );

            databaseContext.ArticleMaterials.AddRange(
                articleDependencyInjection,
                articleLinqFundamentals
            );

            await databaseContext.SaveChangesAsync();
        }

        if (!await databaseContext.Courses.AnyAsync())
        {
            var courseAspNetFundamentals = new Course
            {
                Name = "ASP.NET Core Fundamentals",
                Description = "Essentials: MVC, DI, model binding, validation."
            };

            var courseEfCoreMastery = new Course
            {
                Name = "EF Core Mastery",
                Description = "Relationships, migrations, querying, performance."
            };

            var courseCSharpEssentials = new Course
            {
                Name = "C# Language Essentials",
                Description = "Language basics and object-oriented mindset."
            };

            databaseContext.Courses.AddRange(
                courseAspNetFundamentals,
                courseEfCoreMastery,
                courseCSharpEssentials
            );

            await databaseContext.SaveChangesAsync();

            var allMaterials = await databaseContext.Materials.ToListAsync();
            var allSkills = await databaseContext.Skills.ToListAsync();

            var introToCSharpVideoId = allMaterials
                .OfType<VideoMaterial>()
                .Single(video => video.Title == "Intro to C# and .NET")
                .Id;

            var aspNetMvcVideoId = allMaterials
                .OfType<VideoMaterial>()
                .Single(video => video.Title == "ASP.NET Core MVC in 60 minutes")
                .Id;

            var efRelationshipsVideoId = allMaterials
                .OfType<VideoMaterial>()
                .Single(video => video.Title == "EF Core Relationships Deep Dive")
                .Id;

            var proCSharpBookId = allMaterials
                .OfType<BookMaterial>()
                .Single(book => book.Title == "Pro C# and .NET")
                .Id;

            var efCoreInActionBookId = allMaterials
                .OfType<BookMaterial>()
                .Single(book => book.Title == "EF Core in Action")
                .Id;

            var dependencyInjectionArticleId = allMaterials
                .OfType<ArticleMaterial>()
                .Single(article => article.Title == "What is Dependency Injection in ASP.NET Core?")
                .Id;

            var linqFundamentalsArticleId = allMaterials
                .OfType<ArticleMaterial>()
                .Single(article => article.Title == "LINQ fundamentals")
                .Id;

            var csharpFundamentalsSkillId = allSkills.Single(skill => skill.Name == "C# Fundamentals").Id;
            var oopInCsharpSkillId = allSkills.Single(skill => skill.Name == "OOP in C#").Id;
            var aspNetCoreSkillId = allSkills.Single(skill => skill.Name == "ASP.NET Core").Id;
            var entityFrameworkCoreSkillId = allSkills.Single(skill => skill.Name == "Entity Framework Core").Id;
            var linqSkillId = allSkills.Single(skill => skill.Name == "LINQ").Id;
            var sqlBasicsSkillId = allSkills.Single(skill => skill.Name == "SQL Basics").Id;

            databaseContext.CourseMaterials.AddRange(
                new CourseMaterial { CourseId = courseAspNetFundamentals.Id, MaterialId = aspNetMvcVideoId, RecordStatus = RecordStatus.Active },
                new CourseMaterial { CourseId = courseAspNetFundamentals.Id, MaterialId = dependencyInjectionArticleId, RecordStatus = RecordStatus.Active },
                new CourseMaterial { CourseId = courseAspNetFundamentals.Id, MaterialId = proCSharpBookId, RecordStatus = RecordStatus.Active },

                new CourseMaterial { CourseId = courseEfCoreMastery.Id, MaterialId = efRelationshipsVideoId, RecordStatus = RecordStatus.Active },
                new CourseMaterial { CourseId = courseEfCoreMastery.Id, MaterialId = efCoreInActionBookId, RecordStatus = RecordStatus.Active },
                new CourseMaterial { CourseId = courseEfCoreMastery.Id, MaterialId = dependencyInjectionArticleId, RecordStatus = RecordStatus.Active },

                new CourseMaterial { CourseId = courseCSharpEssentials.Id, MaterialId = introToCSharpVideoId, RecordStatus = RecordStatus.Active },
                new CourseMaterial { CourseId = courseCSharpEssentials.Id, MaterialId = proCSharpBookId, RecordStatus = RecordStatus.Active },
                new CourseMaterial { CourseId = courseCSharpEssentials.Id, MaterialId = linqFundamentalsArticleId, RecordStatus = RecordStatus.Active }
            );

            databaseContext.CourseSkills.AddRange(
                new CourseSkill { CourseId = courseAspNetFundamentals.Id, SkillId = aspNetCoreSkillId, RecordStatus = RecordStatus.Active },
                new CourseSkill { CourseId = courseAspNetFundamentals.Id, SkillId = csharpFundamentalsSkillId, RecordStatus = RecordStatus.Active },
                new CourseSkill { CourseId = courseAspNetFundamentals.Id, SkillId = oopInCsharpSkillId, RecordStatus = RecordStatus.Active },
                new CourseSkill { CourseId = courseAspNetFundamentals.Id, SkillId = linqSkillId, RecordStatus = RecordStatus.Active },

                new CourseSkill { CourseId = courseEfCoreMastery.Id, SkillId = entityFrameworkCoreSkillId, RecordStatus = RecordStatus.Active },
                new CourseSkill { CourseId = courseEfCoreMastery.Id, SkillId = linqSkillId, RecordStatus = RecordStatus.Active },
                new CourseSkill { CourseId = courseEfCoreMastery.Id, SkillId = sqlBasicsSkillId, RecordStatus = RecordStatus.Active },

                new CourseSkill { CourseId = courseCSharpEssentials.Id, SkillId = csharpFundamentalsSkillId, RecordStatus = RecordStatus.Active },
                new CourseSkill { CourseId = courseCSharpEssentials.Id, SkillId = oopInCsharpSkillId, RecordStatus = RecordStatus.Active }
            );

            await databaseContext.SaveChangesAsync();
        }
    }
}