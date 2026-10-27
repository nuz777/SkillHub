using SkillHub.Application.DTOs;

namespace SkillHub.Application.Interfaces;

public interface IWorkshopService
{
    Task<IEnumerable<WorkshopDto>> GetAvailableWorkshopsAsync(CancellationToken cancellationToken = default);
    Task<WorkshopDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<WorkshopDto> CreateWorkshopAsync(CreateWorkshopDto dto, CancellationToken cancellationToken = default);
    Task<IEnumerable<TopWorkshopReportDto>> GetTopWorkshopsReportAsync(CancellationToken cancellationToken = default);
}
