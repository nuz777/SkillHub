using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillHub.Application.DTOs;
using SkillHub.Application.Interfaces;
using SkillHub.Domain.Enums;

namespace SkillHub.Api.Controllers;

[ApiController]
[Route("api/v1/workshops")]
public class WorkshopsController(
    IWorkshopService workshopService,
    IEnrollmentService enrollmentService) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<WorkshopDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<WorkshopDto>>> GetAvailableWorkshops(CancellationToken cancellationToken)
    {
        var workshops = await workshopService.GetAvailableWorkshopsAsync(cancellationToken);
        return Ok(workshops);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(WorkshopDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkshopDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var workshop = await workshopService.GetByIdAsync(id, cancellationToken);
        if (workshop is null)
        {
            return NotFound(new { message = $"Taller con ID '{id}' no encontrado." });
        }
        return Ok(workshop);
    }

    [HttpPost]
    [Authorize(Roles = UserRoles.Employee)]
    [ProducesResponseType(typeof(WorkshopDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkshopDto>> CreateWorkshop([FromBody] CreateWorkshopDto dto, CancellationToken cancellationToken)
    {
        // Si no se proporcionó InstructorEmail explícitamente, se toma automáticamente del usuario autenticado
        if (string.IsNullOrWhiteSpace(dto.InstructorEmail))
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email) 
                            ?? User.FindFirstValue(ClaimTypes.Name) 
                            ?? User.Identity?.Name;
            if (!string.IsNullOrWhiteSpace(userEmail))
            {
                dto = dto with { InstructorEmail = userEmail };
            }
        }

        var created = await workshopService.CreateWorkshopAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = UserRoles.Employee)]
    [ProducesResponseType(typeof(WorkshopDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkshopDto>> UpdateWorkshop(Guid id, [FromBody] UpdateWorkshopDto dto, CancellationToken cancellationToken)
    {
        var userEmail = User.FindFirstValue(ClaimTypes.Email) 
                        ?? User.FindFirstValue(ClaimTypes.Name) 
                        ?? User.Identity?.Name 
                        ?? string.Empty;

        var updated = await workshopService.UpdateWorkshopAsync(id, dto, userEmail, true, cancellationToken);
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = UserRoles.Employee)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteWorkshop(Guid id, CancellationToken cancellationToken)
    {
        var userEmail = User.FindFirstValue(ClaimTypes.Email) 
                        ?? User.FindFirstValue(ClaimTypes.Name) 
                        ?? User.Identity?.Name 
                        ?? string.Empty;

        await workshopService.DeleteWorkshopAsync(id, userEmail, true, cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:guid}/enrollments")]
    [Authorize(Roles = UserRoles.Employee)]
    [ProducesResponseType(typeof(IEnumerable<EnrollmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<EnrollmentDto>>> GetWorkshopEnrollments(Guid id, CancellationToken cancellationToken)
    {
        var enrollments = await enrollmentService.GetWorkshopEnrollmentsAsync(id, cancellationToken);
        return Ok(enrollments);
    }
}
