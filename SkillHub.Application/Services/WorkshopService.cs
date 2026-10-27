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
            StartDate = dto.StartDate.ToUniversalTime(),
            EndDate = dto.EndDate.ToUniversalTime(),
            MaxCapacity = dto.MaxCapacity
        };

        // Reglas de negocio del dominio
        workshop.ValidateForCreation();

        var created = await workshopRepository.AddAsync(workshop, cancellationToken);
        return MapToDto(created);
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
