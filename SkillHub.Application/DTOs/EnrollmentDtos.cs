using System.ComponentModel.DataAnnotations;
using SkillHub.Domain.Enums;

namespace SkillHub.Application.DTOs;

public record EnrollmentDto(
    Guid Id,
    Guid WorkshopId,
    string WorkshopCode,
    string WorkshopTitle,
    string InstructorName,
    string ParticipantName,
    string ParticipantEmail,
    EnrollmentStatus Status,
    DateTime CreatedAt,
    DateTime? CancelledAt,
    string? CancelledBy,
    bool CanCancel
);

public record CreateEnrollmentDto(
    [Required(ErrorMessage = "El ID del taller es obligatorio.")]
    Guid WorkshopId
);

public record CertificateDataDto(
    Guid EnrollmentId,
    string ParticipantName,
    string ParticipantEmail,
    string WorkshopCode,
    string WorkshopTitle,
    string InstructorName,
    DateTime StartDate,
    DateTime EndDate,
    DateTime EnrollmentDate,
    DateTime GeneratedAt
);

public record ImportEnrollmentsResultDto(
    int TotalProcessed,
    int SuccessfulCount,
    int RejectedCount,
    IReadOnlyList<ImportEnrollmentRowErrorDto> RejectedRows
);

public record ImportEnrollmentRowErrorDto(
    int RowNumber,
    string ParticipantEmail,
    string WorkshopCode,
    string Reason
);
