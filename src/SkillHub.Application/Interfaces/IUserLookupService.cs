namespace SkillHub.Application.Interfaces;

public interface IUserLookupService
{
    Task<(string UserId, string FullName, string Role)?> FindUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ExistsUserAsync(string email, CancellationToken cancellationToken = default);
}
