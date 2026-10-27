using SkillHub.Application.DTOs;
using SkillHub.Application.Interfaces;
using SkillHub.Domain.Entities;
using SkillHub.Domain.Exceptions;
using SkillHub.Domain.Interfaces;

namespace SkillHub.Application.Services;

public class WorkshopService(
    IWorkshopRepository workshopRepository,
    IInstructorRepository instructorRepository) : IWorkshopService
{
    public async Task<IEnumerable<WorkshopDto>> GetAvailableWorkshopsAsync(CancellationToken cancellationToken = default)
    {
        var workshops = await workshopRepository.GetAvailableWorkshopsAsync(cancellationToken);
        return workshops.Select(MapToDto);
    }

    public async Task<WorkshopDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var workshop = await workshopRepository.GetByIdAsync(id, cancellationToken);
        return workshop is null ? null : MapToDto(workshop);
    }

    public async Task<WorkshopDto> CreateWorkshopAsync(CreateWorkshopDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.InstructorEmail))
        {
            throw new BusinessRuleException("El correo del instructor es obligatorio.", 400);
        }

        var normalizedEmail = dto.InstructorEmail.Trim().ToLowerInvariant();
        var instructor = await instructorRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

        if (instructor is null)
        {
            throw new BusinessRuleException(
                $"El instructor con correo '{dto.InstructorEmail}' no está registrado en la plataforma.",
                404);
        }

        var normalizedCode = dto.Code.Trim().ToUpperInvariant();
        var codeExists = await workshopRepository.ExistsCodeAsync(normalizedCode, cancellationToken);
        if (codeExists)
        {
            throw new BusinessRuleException($"Ya existe un taller registrado con el código '{normalizedCode}'.", 400);
        }

        var workshop = new Workshop
        {
            Code = normalizedCode,
            Title = dto.Title.Trim(),
            Description = dto.Description?.Trim() ?? string.Empty,
            InstructorId = instructor.Id,
            InstructorEmail = instructor.Email,
            Instructor = instructor,
            StartDate = DateTime.SpecifyKind(dto.StartDate, DateTimeKind.Utc),
            EndDate = DateTime.SpecifyKind(dto.EndDate, DateTimeKind.Utc),
            MaxCapacity = dto.MaxCapacity,
            CreatedAt = DateTime.UtcNow
        };

        // Reglas de negocio del dominio
        workshop.ValidateForCreation();

        var created = await workshopRepository.AddAsync(workshop, cancellationToken);
        return MapToDto(created);
    }

    public async Task<WorkshopDto> UpdateWorkshopAsync(
        Guid id, 
        UpdateWorkshopDto dto, 
        string requestedByEmail, 
        bool isEmployee, 
        CancellationToken cancellationToken = default)
    {
        var workshop = await workshopRepository.GetByIdAsync(id, cancellationToken);
        if (workshop is null)
        {
            throw new BusinessRuleException("El taller especificado no existe.", 404);
        }

        var normalizedEmail = requestedByEmail.Trim().ToLowerInvariant();
        var isOwner = string.Equals(workshop.InstructorEmail, normalizedEmail, StringComparison.OrdinalIgnoreCase);
        var isAdmin = normalizedEmail.Contains("admin");

        if (!isOwner && !isAdmin)
        {
            throw new BusinessRuleException("Solo el instructor a cargo o un administrador pueden modificar este taller.", 403);
        }

        if (workshop.HasStarted)
        {
            throw new BusinessRuleException("No es posible modificar las fechas u horarios de un taller que ya ha iniciado.", 400);
        }

        if (dto.MaxCapacity < workshop.ActiveEnrollmentsCount)
        {
            throw new BusinessRuleException(
                $"El cupo máximo ({dto.MaxCapacity}) no puede ser menor al número de participantes ya inscritos ({workshop.ActiveEnrollmentsCount}).", 
                400);
        }

        if (dto.StartDate.ToUniversalTime() <= DateTime.UtcNow)
        {
            throw new BusinessRuleException("La nueva fecha de inicio debe ser futura.", 400);
        }

        if (dto.EndDate <= dto.StartDate)
        {
            throw new BusinessRuleException("La fecha de finalización debe ser posterior a la de inicio.", 400);
        }

        workshop.Title = dto.Title.Trim();
        workshop.Description = dto.Description?.Trim() ?? string.Empty;
        workshop.StartDate = DateTime.SpecifyKind(dto.StartDate, DateTimeKind.Utc);
        workshop.EndDate = DateTime.SpecifyKind(dto.EndDate, DateTimeKind.Utc);
        workshop.MaxCapacity = dto.MaxCapacity;
        workshop.UpdatedAt = DateTime.UtcNow;

        await workshopRepository.UpdateAsync(workshop, cancellationToken);
        return MapToDto(workshop);
    }

    public async Task DeleteWorkshopAsync(
        Guid id, 
        string requestedByEmail, 
        bool isEmployee, 
        CancellationToken cancellationToken = default)
    {
        var workshop = await workshopRepository.GetByIdAsync(id, cancellationToken);
        if (workshop is null)
        {
            throw new BusinessRuleException("El taller especificado no existe.", 404);
        }

        var normalizedEmail = requestedByEmail.Trim().ToLowerInvariant();
        var isOwner = string.Equals(workshop.InstructorEmail, normalizedEmail, StringComparison.OrdinalIgnoreCase);
        var isAdmin = normalizedEmail.Contains("admin");

        if (!isOwner && !isAdmin)
        {
            throw new BusinessRuleException("Solo el instructor a cargo o un administrador pueden eliminar este taller.", 403);
        }

        if (workshop.HasStarted)
        {
            throw new BusinessRuleException("No es posible eliminar un taller que ya ha comenzado.", 400);
        }

        if (workshop.ActiveEnrollmentsCount > 0)
        {
            throw new BusinessRuleException(
                $"No es posible eliminar el taller porque actualmente tiene {workshop.ActiveEnrollmentsCount} participantes inscritos activos.", 
                400);
        }

        await workshopRepository.DeleteAsync(workshop, cancellationToken);
    }

    public async Task<IEnumerable<TopWorkshopReportDto>> GetTopWorkshopsReportAsync(CancellationToken cancellationToken = default)
    {
        var workshops = await workshopRepository.GetTopWorkshopsAsync(100, cancellationToken);

        return workshops.Select(w =>
        {
            var activeCount = w.ActiveEnrollmentsCount;
            var rate = w.MaxCapacity > 0 ? Math.Round((double)activeCount / w.MaxCapacity * 100, 2) : 0;
            return new TopWorkshopReportDto(
                w.Id,
                w.Code,
                w.Title,
                w.Instructor?.FullName ?? w.InstructorEmail,
                activeCount,
                w.MaxCapacity,
                rate
            );
        });
    }

    public static WorkshopDto MapToDto(Workshop w)
    {
        return new WorkshopDto(
            w.Id,
            w.Code,
            w.Title,
            w.Description,
            w.Instructor?.FullName ?? w.InstructorEmail,
            w.InstructorEmail,
            w.StartDate,
            w.EndDate,
            w.MaxCapacity,
            w.AvailableSpots,
            w.ActiveEnrollmentsCount,
            w.HasStarted
        );
    }
}
