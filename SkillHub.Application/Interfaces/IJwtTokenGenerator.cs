namespace SkillHub.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(string userId, string email, string fullName, string role, out DateTime expiresAt);
}
