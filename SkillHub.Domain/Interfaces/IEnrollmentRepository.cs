using SkillHub.Domain.Entities;

namespace SkillHub.Domain.Interfaces;

public interface IEnrollmentRepository
{
    Task<Enrollment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Enrollment>> GetByParticipantEmailAsync(string participantEmail, CancellationToken cancellationToken = default);
    Task<IEnumerable<Enrollment>> GetActiveByParticipantEmailAsync(string participantEmail, CancellationToken cancellationToken = default);
    Task<IEnumerable<Enrollment>> GetByWorkshopIdAsync(Guid workshopId, CancellationToken cancellationToken = default);
    Task<bool> HasActiveEnrollmentAsync(string participantEmail, Guid workshopId, CancellationToken cancellationToken = default);
    Task<Enrollment> AddAsync(Enrollment enrollment, CancellationToken cancellationToken = default);
    Task UpdateAsync(Enrollment enrollment, CancellationToken cancellationToken = default);
}
