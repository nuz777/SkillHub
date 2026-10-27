using SkillHub.Domain.Entities;

namespace SkillHub.Domain.Interfaces;

public interface IInstructorRepository
{
    Task<Instructor?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Instructor?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IEnumerable<Instructor>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Instructor> AddAsync(Instructor instructor, CancellationToken cancellationToken = default);
}
