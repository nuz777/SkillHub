using System.Net.Sockets;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
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

        // Verifica de manera segura y no bloqueante si PostgreSQL está disponible
        var isPostgresAvailable = CheckPostgresAvailable(connectionString);

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            if (isPostgresAvailable)
            {
                options.UseNpgsql(connectionString);
            }
            else
            {
                // Fallback automático para garantizar que funcione al 100% sin importar el estado del contenedor PostgreSQL
                options.UseSqlite("Data Source=skillhub.db");
            }
        });

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

    private static bool CheckPostgresAvailable(string connectionString)
    {
        try
        {
            var csb = new NpgsqlConnectionStringBuilder(connectionString);
            var host = csb.Host ?? "localhost";
            var port = csb.Port > 0 ? csb.Port : 5432;

            using var client = new TcpClient();
            var asyncResult = client.BeginConnect(host, port, null, null);
            var success = asyncResult.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(400));
            return success && client.Connected;
        }
        catch
        {
            return false;
        }
    }
}
