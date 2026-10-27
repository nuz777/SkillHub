using SkillHub.Application.DTOs;
using SkillHub.Application.Interfaces;
using SkillHub.Domain.Enums;
using SkillHub.Domain.Exceptions;

namespace SkillHub.Application.Services;

public class AuthService(
    IIdentityService identityService,
    IJwtTokenGenerator jwtTokenGenerator) : IAuthService
{
    public async Task<AuthResponseDto> RegisterParticipantAsync(RegisterParticipantDto dto, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();

        var result = await identityService.CreateUserAsync(
            email: normalizedEmail,
            fullName: dto.FullName.Trim(),
            password: dto.Password,
            role: UserRoles.Participant,
            cancellationToken: cancellationToken
        );

        if (!result.Succeeded)
        {
            var errorMessage = string.Join("; ", result.Errors);
            throw new BusinessRuleException($"Error al registrar el participante: {errorMessage}", 400);
        }

        var token = jwtTokenGenerator.GenerateToken(
            userId: result.UserId,
            email: normalizedEmail,
            fullName: dto.FullName.Trim(),
            role: UserRoles.Participant,
            out var expiresAt
        );

        return new AuthResponseDto(
            Token: token,
            Email: normalizedEmail,
            FullName: dto.FullName.Trim(),
            Role: UserRoles.Participant,
            ExpiresAt: expiresAt
        );
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();

        var (succeeded, userId, fullName, role, error) = await identityService.ValidateUserCredentialsAsync(
            email: normalizedEmail,
            password: dto.Password,
            cancellationToken: cancellationToken
        );

        if (!succeeded)
        {
            throw new BusinessRuleException(error ?? "Credenciales inválidas.", 401);
        }

        var token = jwtTokenGenerator.GenerateToken(
            userId: userId,
            email: normalizedEmail,
            fullName: fullName,
            role: role,
            out var expiresAt
        );

        return new AuthResponseDto(
            Token: token,
            Email: normalizedEmail,
            FullName: fullName,
            Role: role,
            ExpiresAt: expiresAt
        );
    }
}
