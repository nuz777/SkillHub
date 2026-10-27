using SkillHub.Application.DTOs;
using SkillHub.Application.Interfaces;
using SkillHub.Domain.Entities;
using SkillHub.Domain.Enums;
using SkillHub.Domain.Exceptions;
using SkillHub.Domain.Interfaces;

namespace SkillHub.Application.Services;

public class EnrollmentService(
    IEnrollmentRepository enrollmentRepository,
    IWorkshopRepository workshopRepository,
    IUserLookupService userLookupService,
    IExcelReader excelReader,
    ICertificatePdfGenerator certificatePdfGenerator) : IEnrollmentService
{
    public async Task<EnrollmentDto> EnrollAsync(string participantEmail, Guid workshopId, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = participantEmail.Trim().ToLowerInvariant();

        var user = await userLookupService.FindUserByEmailAsync(normalizedEmail, cancellationToken);
        if (user is null)
        {
            throw new BusinessRuleException($"El participante con correo '{participantEmail}' no existe en la plataforma.", 404);
        }

        var workshop = await workshopRepository.GetByIdAsync(workshopId, cancellationToken);
        if (workshop is null)
        {
            throw new BusinessRuleException($"El taller especificado no existe.", 404);
        }

        ValidateEnrollmentRules(workshop, user.Value.FullName, normalizedEmail, await enrollmentRepository.GetActiveByParticipantEmailAsync(normalizedEmail, cancellationToken));

        var enrollment = new Enrollment
        {
            WorkshopId = workshop.Id,
            Workshop = workshop,
            ParticipantId = user.Value.UserId,
            ParticipantName = user.Value.FullName,
            ParticipantEmail = normalizedEmail,
            Status = EnrollmentStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        var created = await enrollmentRepository.AddAsync(enrollment, cancellationToken);
        return MapToDto(created);
    }

    public async Task<IEnumerable<EnrollmentDto>> GetMyEnrollmentsAsync(string participantEmail, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = participantEmail.Trim().ToLowerInvariant();
        var enrollments = await enrollmentRepository.GetByParticipantEmailAsync(normalizedEmail, cancellationToken);
        return enrollments.Select(MapToDto);
    }

    public async Task<IEnumerable<EnrollmentDto>> GetWorkshopEnrollmentsAsync(Guid workshopId, CancellationToken cancellationToken = default)
    {
        var workshop = await workshopRepository.GetByIdAsync(workshopId, cancellationToken);
        if (workshop is null)
        {
            throw new BusinessRuleException("El taller especificado no existe.", 404);
        }

        var enrollments = await enrollmentRepository.GetByWorkshopIdAsync(workshopId, cancellationToken);
        return enrollments.Select(MapToDto);
    }

    public async Task<EnrollmentDto> CancelEnrollmentAsync(Guid enrollmentId, string requestedByEmail, bool isEmployee, CancellationToken cancellationToken = default)
    {
        var enrollment = await enrollmentRepository.GetByIdAsync(enrollmentId, cancellationToken);
        if (enrollment is null)
        {
            throw new BusinessRuleException("La inscripción solicitada no existe.", 404);
        }

        var normalizedEmail = requestedByEmail.Trim().ToLowerInvariant();

        // Validar propiedad de la inscripción para participantes
        if (!isEmployee && !string.Equals(enrollment.ParticipantEmail, normalizedEmail, StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessRuleException("No tienes permisos para cancelar una inscripción que no es tuya.", 403);
        }

        // Ejecutar cancelación en el dominio (aplica reglas de tiempo)
        enrollment.Cancel(normalizedEmail, isEmployee);

        await enrollmentRepository.UpdateAsync(enrollment, cancellationToken);
        return MapToDto(enrollment);
    }

    public async Task<ImportEnrollmentsResultDto> ImportFromExcelAsync(Stream fileStream, string employeeEmail, CancellationToken cancellationToken = default)
    {
        var rows = await excelReader.ReadEnrollmentRowsAsync(fileStream, cancellationToken);
        var rejectedRows = new List<ImportEnrollmentRowErrorDto>();
        var successfulCount = 0;

        foreach (var row in rows)
        {
            try
            {
                var participantEmail = row.ParticipantEmail?.Trim().ToLowerInvariant();
                var workshopCode = row.WorkshopCode?.Trim().ToUpperInvariant();

                if (string.IsNullOrWhiteSpace(participantEmail) || string.IsNullOrWhiteSpace(workshopCode))
                {
                    rejectedRows.Add(new ImportEnrollmentRowErrorDto(
                        row.RowNumber,
                        row.ParticipantEmail ?? string.Empty,
                        row.WorkshopCode ?? string.Empty,
                        "El correo del participante y el código del taller son campos requeridos."
                    ));
                    continue;
                }

                var user = await userLookupService.FindUserByEmailAsync(participantEmail, cancellationToken);
                if (user is null)
                {
                    rejectedRows.Add(new ImportEnrollmentRowErrorDto(
                        row.RowNumber,
                        participantEmail,
                        workshopCode,
                        $"El participante con correo '{participantEmail}' no existe en la plataforma."
                    ));
                    continue;
                }

                var workshop = await workshopRepository.GetByCodeAsync(workshopCode, cancellationToken);
                if (workshop is null)
                {
                    rejectedRows.Add(new ImportEnrollmentRowErrorDto(
                        row.RowNumber,
                        participantEmail,
                        workshopCode,
                        $"El taller con código '{workshopCode}' no existe."
                    ));
                    continue;
                }

                var activeEnrollments = await enrollmentRepository.GetActiveByParticipantEmailAsync(participantEmail, cancellationToken);
                ValidateEnrollmentRules(workshop, user.Value.FullName, participantEmail, activeEnrollments);

                var enrollment = new Enrollment
                {
                    WorkshopId = workshop.Id,
                    Workshop = workshop,
                    ParticipantId = user.Value.UserId,
                    ParticipantName = user.Value.FullName,
                    ParticipantEmail = participantEmail,
                    Status = EnrollmentStatus.Active,
                    CreatedAt = DateTime.UtcNow
                };

                await enrollmentRepository.AddAsync(enrollment, cancellationToken);
                successfulCount++;
            }
            catch (BusinessRuleException ex)
            {
                rejectedRows.Add(new ImportEnrollmentRowErrorDto(
                    row.RowNumber,
                    row.ParticipantEmail ?? string.Empty,
                    row.WorkshopCode ?? string.Empty,
                    ex.Message
                ));
            }
            catch (Exception ex)
            {
                rejectedRows.Add(new ImportEnrollmentRowErrorDto(
                    row.RowNumber,
                    row.ParticipantEmail ?? string.Empty,
                    row.WorkshopCode ?? string.Empty,
                    $"Error al procesar la fila: {ex.Message}"
                ));
            }
        }

        return new ImportEnrollmentsResultDto(
            TotalProcessed: rows.Count,
            SuccessfulCount: successfulCount,
            RejectedCount: rejectedRows.Count,
            RejectedRows: rejectedRows
        );
    }

    public async Task<byte[]> GetCertificatePdfAsync(Guid enrollmentId, string requestedByEmail, bool isEmployee, CancellationToken cancellationToken = default)
    {
        var enrollment = await enrollmentRepository.GetByIdAsync(enrollmentId, cancellationToken);
        if (enrollment is null)
        {
            throw new BusinessRuleException("La inscripción solicitada no existe.", 404);
        }

        var normalizedEmail = requestedByEmail.Trim().ToLowerInvariant();
        if (!isEmployee && !string.Equals(enrollment.ParticipantEmail, normalizedEmail, StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessRuleException("Solo puedes descargar la constancia de tus propias inscripciones.", 403);
        }

        if (enrollment.Status != EnrollmentStatus.Active)
        {
            throw new BusinessRuleException("Solo se puede generar la constancia para inscripciones activas.", 400);
        }

        var workshop = enrollment.Workshop;
        if (workshop is null)
        {
            throw new BusinessRuleException("No se encontró la información del taller asociado.", 404);
        }

        var certificateData = new CertificateDataDto(
            EnrollmentId: enrollment.Id,
            ParticipantName: enrollment.ParticipantName,
            ParticipantEmail: enrollment.ParticipantEmail,
            WorkshopCode: workshop.Code,
            WorkshopTitle: workshop.Title,
            InstructorName: workshop.Instructor?.FullName ?? workshop.InstructorEmail,
            StartDate: workshop.StartDate,
            EndDate: workshop.EndDate,
            EnrollmentDate: enrollment.CreatedAt,
            GeneratedAt: DateTime.UtcNow
        );

        return await certificatePdfGenerator.GenerateAsync(certificateData, cancellationToken);
    }

    private static void ValidateEnrollmentRules(
        Workshop workshop,
        string participantName,
        string participantEmail,
        IEnumerable<Enrollment> participantActiveEnrollments)
    {
        // Regla 4: Solo talleres futuros
        if (workshop.HasStarted)
        {
            throw new BusinessRuleException("No es posible inscribirse en un taller que ya comenzó.");
        }

        // Regla 1: Cupo máximo
        if (!workshop.HasAvailableSpots())
        {
            throw new BusinessRuleException($"No se puede superar el cupo máximo ({workshop.MaxCapacity}) del taller '{workshop.Title}'. No hay cupos disponibles.");
        }

        // Regla 2: Sin duplicados
        var isAlreadyEnrolled = participantActiveEnrollments.Any(e => e.WorkshopId == workshop.Id);
        if (isAlreadyEnrolled)
        {
            throw new BusinessRuleException("El participante ya tiene una inscripción activa en este taller.");
        }

        // Regla 3: Sin cruces de horario
        var conflictingEnrollment = participantActiveEnrollments
            .FirstOrDefault(e => e.Workshop != null && e.Workshop.OverlapsWith(workshop.StartDate, workshop.EndDate));

        if (conflictingEnrollment != null && conflictingEnrollment.Workshop != null)
        {
            throw new BusinessRuleException(
                $"Cruce de horario: El participante ya está inscrito en el taller '{conflictingEnrollment.Workshop.Title}' ({conflictingEnrollment.Workshop.Code}), el cual se cruza con el horario de este taller ({workshop.StartDate:dd/MM/yyyy HH:mm} - {workshop.EndDate:HH:mm}).");
        }
    }

    private static EnrollmentDto MapToDto(Enrollment e)
    {
        var workshop = e.Workshop;
        var canCancel = e.Status == EnrollmentStatus.Active && workshop != null && workshop.CanBeCancelledByParticipant();

        return new EnrollmentDto(
            Id: e.Id,
            WorkshopId: e.WorkshopId,
            WorkshopCode: workshop?.Code ?? string.Empty,
            WorkshopTitle: workshop?.Title ?? string.Empty,
            InstructorName: workshop?.Instructor?.FullName ?? workshop?.InstructorEmail ?? string.Empty,
            ParticipantName: e.ParticipantName,
            ParticipantEmail: e.ParticipantEmail,
            Status: e.Status,
            CreatedAt: e.CreatedAt,
            CancelledAt: e.CancelledAt,
            CancelledBy: e.CancelledBy,
            CanCancel: canCancel
        );
    }
}
