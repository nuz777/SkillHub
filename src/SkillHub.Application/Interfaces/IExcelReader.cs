namespace SkillHub.Application.Interfaces;

public record ExcelWorkshopRow(
    int RowNumber,
    string Code,
    string Title,
    string Description,
    string InstructorEmail,
    DateTime? StartDate,
    DateTime? EndDate,
    int? Capacity
);

public interface IExcelReader
{
    Task<IReadOnlyList<(int RowNumber, string ParticipantEmail, string WorkshopCode)>> ReadEnrollmentRowsAsync(Stream fileStream, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExcelWorkshopRow>> ReadWorkshopRowsAsync(Stream fileStream, CancellationToken cancellationToken = default);
}
