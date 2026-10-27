namespace SkillHub.Application.Interfaces;

public interface IExcelReader
{
    Task<IReadOnlyList<(int RowNumber, string ParticipantEmail, string WorkshopCode)>> ReadEnrollmentRowsAsync(Stream fileStream, CancellationToken cancellationToken = default);
}
