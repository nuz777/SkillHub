using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SkillHub.Application.Interfaces;
using SkillHub.Domain.Enums;
using SkillHub.Infrastructure.Identity;

namespace SkillHub.Infrastructure.Identity;

public class IdentityService(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager) : IIdentityService, IUserLookupService
{
    public async Task<(bool Succeeded, string UserId, IEnumerable<string> Errors)> CreateUserAsync(
        string email,
        string fullName,
        string password,
        string role,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        var existingUser = await userManager.FindByEmailAsync(normalizedEmail);
        if (existingUser != null)
        {
            return (false, string.Empty, ["Ya existe un usuario con este correo electrónico."]);
        }

        var user = new ApplicationUser
        {
            UserName = normalizedEmail,
            Email = normalizedEmail,
            FullName = fullName.Trim()
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            return (false, string.Empty, result.Errors.Select(e => e.Description));
        }

        // Asegurar que el rol exista
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }

        await userManager.AddToRoleAsync(user, role);

        return (true, user.Id, []);
    }

    public async Task<(bool Succeeded, string UserId, string FullName, string Role, string? Error)> ValidateUserCredentialsAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await userManager.FindByEmailAsync(normalizedEmail);

        if (user is null)
        {
            return (false, string.Empty, string.Empty, string.Empty, "Correo electrónico o contraseña incorrectos.");
        }

        var isValidPassword = await userManager.CheckPasswordAsync(user, password);
        if (!isValidPassword)
        {
            return (false, string.Empty, string.Empty, string.Empty, "Correo electrónico o contraseña incorrectos.");
        }

        var roles = await userManager.GetRolesAsync(user);
        var primaryRole = roles.FirstOrDefault() ?? UserRoles.Participant;

        return (true, user.Id, user.FullName, primaryRole, null);
    }

    public async Task<(string UserId, string FullName, string Role)?> FindUserByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await userManager.FindByEmailAsync(normalizedEmail);
        if (user is null)
            return null;

        var roles = await userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? UserRoles.Participant;

        return (user.Id, user.FullName, role);
    }

    public async Task<bool> ExistsUserAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        return await userManager.Users.AnyAsync(u => u.Email == normalizedEmail, cancellationToken);
    }
}
