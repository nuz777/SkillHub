using SkillHub.Application.DTOs;

namespace SkillHub.Application.Interfaces;

public interface ICertificatePdfGenerator
{
    Task<byte[]> GenerateAsync(CertificateDataDto data, CancellationToken cancellationToken = default);
}
