using SkillHub.Application.DTOs;

namespace SkillHub.Application.Interfaces;

public interface IEnrollmentService
{
    Task<EnrollmentDto> EnrollAsync(string participantEmail, Guid workshopId, CancellationToken cancellationToken = default);
    Task<IEnumerable<EnrollmentDto>> GetMyEnrollmentsAsync(string participantEmail, CancellationToken cancellationToken = default);
    Task<IEnumerable<EnrollmentDto>> GetWorkshopEnrollmentsAsync(Guid workshopId, CancellationToken cancellationToken = default);
    Task<EnrollmentDto> CancelEnrollmentAsync(Guid enrollmentId, string requestedByEmail, bool isEmployee, CancellationToken cancellationToken = default);
    Task<ImportEnrollmentsResultDto> ImportFromExcelAsync(Stream fileStream, string employeeEmail, CancellationToken cancellationToken = default);
    Task<byte[]> GetCertificatePdfAsync(Guid enrollmentId, string requestedByEmail, bool isEmployee, CancellationToken cancellationToken = default);
    Task<int> ClearAllEnrollmentsAsync(CancellationToken cancellationToken = default);
}
