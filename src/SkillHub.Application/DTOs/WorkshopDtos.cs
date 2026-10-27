using System.ComponentModel.DataAnnotations;

namespace SkillHub.Application.DTOs;

public record WorkshopDto(
    Guid Id,
    string Code,
    string Title,
    string Description,
    string InstructorName,
    string InstructorEmail,
    DateTime StartDate,
    DateTime EndDate,
    int MaxCapacity,
    int AvailableSpots,
    int ActiveEnrollmentsCount,
    bool HasStarted
);

public record CreateWorkshopDto(
    [Required(ErrorMessage = "El código del taller es obligatorio.")]
    [MaxLength(20)]
    string Code,

    [Required(ErrorMessage = "El título es obligatorio.")]
    [MaxLength(150)]
    string Title,

    [MaxLength(1000)]
    string Description,

    [EmailAddress(ErrorMessage = "El correo del instructor debe ser válido.")]
    string? InstructorEmail = null,

    [Required(ErrorMessage = "La fecha y hora de inicio es requerida.")]
    DateTime StartDate = default,

    [Required(ErrorMessage = "La fecha y hora de fin es requerida.")]
    DateTime EndDate = default,

    [Range(1, 500, ErrorMessage = "El cupo debe ser de al menos 1.")]
    int MaxCapacity = 15
);

public record UpdateWorkshopDto(
    [Required(ErrorMessage = "El título es obligatorio.")]
    [MaxLength(150)]
    string Title,

    [MaxLength(1000)]
    string Description,

    [Required(ErrorMessage = "La fecha y hora de inicio es requerida.")]
    DateTime StartDate,

    [Required(ErrorMessage = "La fecha y hora de fin es requerida.")]
    DateTime EndDate,

    [Range(1, 500, ErrorMessage = "El cupo debe estar entre 1 y 500.")]
    int MaxCapacity
);

public record TopWorkshopReportDto(
    Guid Id,
    string Code,
    string Title,
    string InstructorName,
    int ActiveEnrollmentsCount,
    int MaxCapacity,
    double OccupancyPercentage
);
