namespace SkillHub.Application.Interfaces;

public interface IIdentityService
{
    Task<(bool Succeeded, string UserId, IEnumerable<string> Errors)> CreateUserAsync(string email, string fullName, string password, string role, CancellationToken cancellationToken = default);
    Task<(bool Succeeded, string UserId, string FullName, string Role, string? Error)> ValidateUserCredentialsAsync(string email, string password, CancellationToken cancellationToken = default);
}
