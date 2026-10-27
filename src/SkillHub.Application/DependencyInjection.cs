using Microsoft.Extensions.DependencyInjection;
using SkillHub.Application.Interfaces;
using SkillHub.Application.Services;

namespace SkillHub.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IWorkshopService, WorkshopService>();
        services.AddScoped<IEnrollmentService, EnrollmentService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
