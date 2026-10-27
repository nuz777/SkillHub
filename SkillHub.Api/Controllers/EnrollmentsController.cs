using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillHub.Application.DTOs;
using SkillHub.Application.Interfaces;
using SkillHub.Domain.Enums;

namespace SkillHub.Api.Controllers;

[ApiController]
[Route("api/v1/enrollments")]
[Authorize]
public class EnrollmentsController(IEnrollmentService enrollmentService) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = UserRoles.Participant)]
    [ProducesResponseType(typeof(EnrollmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EnrollmentDto>> Enroll([FromBody] CreateEnrollmentDto dto, CancellationToken cancellationToken)
    {
        var userEmail = User.FindFirstValue(ClaimTypes.Email) 
                        ?? User.FindFirstValue(ClaimTypes.Name);

        if (string.IsNullOrEmpty(userEmail))
        {
            return Unauthorized(new { message = "No se pudo identificar el correo del participante autenticado." });
        }

        var enrollment = await enrollmentService.EnrollAsync(userEmail, dto.WorkshopId, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, enrollment);
    }

    [HttpGet("mine")]
    [Authorize(Roles = UserRoles.Participant)]
    [ProducesResponseType(typeof(IEnumerable<EnrollmentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<EnrollmentDto>>> GetMyEnrollments(CancellationToken cancellationToken)
    {
        var userEmail = User.FindFirstValue(ClaimTypes.Email) 
                        ?? User.FindFirstValue(ClaimTypes.Name);

        if (string.IsNullOrEmpty(userEmail))
        {
            return Unauthorized(new { message = "No se pudo identificar el correo del participante autenticado." });
        }

        var enrollments = await enrollmentService.GetMyEnrollmentsAsync(userEmail, cancellationToken);
        return Ok(enrollments);
    }

    [HttpPatch("{id:guid}/cancel")]
    [ProducesResponseType(typeof(EnrollmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EnrollmentDto>> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var userEmail = User.FindFirstValue(ClaimTypes.Email) 
                        ?? User.FindFirstValue(ClaimTypes.Name) 
                        ?? string.Empty;

        var isEmployee = User.IsInRole(UserRoles.Employee);

        var result = await enrollmentService.CancelEnrollmentAsync(id, userEmail, isEmployee, cancellationToken);
        return Ok(result);
    }

    [HttpPost("import")]
    [Authorize(Roles = UserRoles.Employee)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ImportEnrollmentsResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ImportEnrollmentsResultDto>> Import(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { message = "Debes proporcionar un archivo Excel (.xlsx) válido." });
        }

        var userEmail = User.FindFirstValue(ClaimTypes.Email) 
                        ?? User.FindFirstValue(ClaimTypes.Name) 
                        ?? "employee@skillhub.com";

        await using var stream = file.OpenReadStream();
        var result = await enrollmentService.ImportFromExcelAsync(stream, userEmail, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}/certificate")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCertificate(Guid id, CancellationToken cancellationToken)
    {
        var userEmail = User.FindFirstValue(ClaimTypes.Email) 
                        ?? User.FindFirstValue(ClaimTypes.Name) 
                        ?? string.Empty;

        var isEmployee = User.IsInRole(UserRoles.Employee);

        var pdfBytes = await enrollmentService.GetCertificatePdfAsync(id, userEmail, isEmployee, cancellationToken);
        return File(pdfBytes, "application/pdf", $"Constancia-Inscripcion-{id}.pdf");
    }
}
