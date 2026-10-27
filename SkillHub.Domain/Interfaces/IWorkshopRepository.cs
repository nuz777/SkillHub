using SkillHub.Domain.Entities;

namespace SkillHub.Domain.Interfaces;

public interface IWorkshopRepository
{
    Task<IEnumerable<Workshop>> GetAvailableWorkshopsAsync(CancellationToken cancellationToken = default);
    Task<Workshop?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Workshop?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<bool> ExistsCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IEnumerable<Workshop>> GetTopWorkshopsAsync(int count = 10, CancellationToken cancellationToken = default);
    Task<Workshop> AddAsync(Workshop workshop, CancellationToken cancellationToken = default);
    Task UpdateAsync(Workshop workshop, CancellationToken cancellationToken = default);
}
