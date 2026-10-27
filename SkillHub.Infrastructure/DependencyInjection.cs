using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SkillHub.Application.Interfaces;
using SkillHub.Domain.Interfaces;
using SkillHub.Infrastructure.Identity;
using SkillHub.Infrastructure.Persistence;
using SkillHub.Infrastructure.Repositories;
using SkillHub.Infrastructure.Security;
using SkillHub.Infrastructure.Services;

namespace SkillHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=skillhub_db;Username=postgres;Password=postgres";

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireDigit = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        // Configuración JWT
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        // Servicios de Identidad y Búsqueda
        services.AddScoped<IdentityService>();
        services.AddScoped<IIdentityService>(sp => sp.GetRequiredService<IdentityService>());
        services.AddScoped<IUserLookupService>(sp => sp.GetRequiredService<IdentityService>());

        // Repositorios
        services.AddScoped<IWorkshopRepository, WorkshopRepository>();
        services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
        services.AddScoped<IInstructorRepository, InstructorRepository>();

        // Servicios externos (Excel & PDF)
        services.AddScoped<IExcelReader, ExcelReader>();
        services.AddScoped<ICertificatePdfGenerator, QuestPdfCertificateGenerator>();

        return services;
    }
}
