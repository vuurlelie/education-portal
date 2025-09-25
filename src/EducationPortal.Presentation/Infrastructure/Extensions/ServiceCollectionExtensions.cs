using EducationPortal.BusinessLogic.Abstractions;
using EducationPortal.BusinessLogic.Services;
using EducationPortal.DataAccess;
using EducationPortal.DataAccess.Abstractions;
using EducationPortal.DataAccess.Entities;
using EducationPortal.DataAccess.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using FluentValidation;
using FluentValidation.AspNetCore;
using EducationPortal.Presentation.Validators;

namespace EducationPortal.Presentation.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAppInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllersWithViews();
        services.AddRazorPages();

        services.AddFluentValidationAutoValidation();
        services.AddFluentValidationClientsideAdapters();
        services.AddValidatorsFromAssemblyContaining<IdRouteModelValidator>();

        services.AddSingleton<TimeProvider>(TimeProvider.System);

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
            );
        });

        services.AddScoped<IAppDbContext>(serviceProvider => serviceProvider.GetRequiredService<AppDbContext>());

        services.AddHttpContextAccessor();

        return services;
    }

    public static IServiceCollection AddIdentityWithUi(this IServiceCollection services)
    {
        services
            .AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Account/Login";
            options.LogoutPath = "/Account/Logout";
            options.AccessDeniedPath = "/Account/AccessDenied";
            options.SlidingExpiration = true;
        });

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IMaterialRepository, MaterialRepository>();
        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<ISkillRepository, SkillRepository>();
        services.AddScoped<IUserCourseRepository, UserCourseRepository>();
        services.AddScoped<IUserMaterialRepository, UserMaterialRepository>();
        services.AddScoped<IUserSkillRepository, UserSkillRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<IMaterialService, MaterialService>();
        services.AddScoped<ISkillService, SkillService>();
        services.AddScoped<IEnrollmentService, EnrollmentService>();
        services.AddScoped<IProfileService, ProfileService>();

        return services;
    }

    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "EducationPortal API",
                Version = "v1",
                Description = "API documentation for the EducationPortal."
            });
        });

        return services;
    }
}