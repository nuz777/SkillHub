using SkillHub.Application.DTOs;

namespace SkillHub.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterParticipantAsync(RegisterParticipantDto dto, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default);
}
