using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillHub.Application.Interfaces;
using SkillHub.Domain.Enums;
using SkillHub.Domain.Exceptions;

namespace SkillHub.Web.Controllers;

[Authorize]
public class EnrollmentsController(IEnrollmentService enrollmentService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var userEmail = User.FindFirstValue(ClaimTypes.Email) 
                        ?? User.FindFirstValue(ClaimTypes.Name) 
                        ?? string.Empty;

        var enrollments = await enrollmentService.GetMyEnrollmentsAsync(userEmail, cancellationToken);
        return View(enrollments);
    }

    [HttpPost]
    [Authorize(Roles = UserRoles.Participant)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enroll(Guid workshopId, CancellationToken cancellationToken)
    {
        var userEmail = User.FindFirstValue(ClaimTypes.Email) 
                        ?? User.FindFirstValue(ClaimTypes.Name) 
                        ?? string.Empty;

        try
        {
            await enrollmentService.EnrollAsync(userEmail, workshopId, cancellationToken);
            TempData["Success"] = "¡Te has inscrito exitosamente en el taller!";
            return RedirectToAction(nameof(Index));
        }
        catch (BusinessRuleException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction("Details", "Workshops", new { id = workshopId });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var userEmail = User.FindFirstValue(ClaimTypes.Email) 
                        ?? User.FindFirstValue(ClaimTypes.Name) 
                        ?? string.Empty;

        var isEmployee = User.IsInRole(UserRoles.Employee);

        try
        {
            await enrollmentService.CancelEnrollmentAsync(id, userEmail, isEmployee, cancellationToken);
            TempData["Success"] = "La inscripción ha sido cancelada correctamente y el cupo ha sido liberado.";
        }
        catch (BusinessRuleException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Certificate(Guid id, [FromQuery] bool download = false, CancellationToken cancellationToken = default)
    {
        var userEmail = User.FindFirstValue(ClaimTypes.Email) 
                        ?? User.FindFirstValue(ClaimTypes.Name) 
                        ?? string.Empty;

        var isEmployee = User.IsInRole(UserRoles.Employee);

        try
        {
            var pdfBytes = await enrollmentService.GetCertificatePdfAsync(id, userEmail, isEmployee, cancellationToken);
            if (download)
            {
                return File(pdfBytes, "application/pdf", $"Constancia-Inscripcion-{id}.pdf");
            }

            Response.Headers.Append("Content-Disposition", $"inline; filename=Constancia-Inscripcion-{id}.pdf");
            return File(pdfBytes, "application/pdf");
        }
        catch (BusinessRuleException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    [Authorize(Roles = UserRoles.Employee)]
    public IActionResult Import()
    {
        return View();
    }

    [HttpPost]
    [Authorize(Roles = UserRoles.Employee)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Import(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            ModelState.AddModelError(string.Empty, "Debes seleccionar un archivo Excel (.xlsx) válido.");
            return View();
        }

        var userEmail = User.FindFirstValue(ClaimTypes.Email) 
                        ?? User.FindFirstValue(ClaimTypes.Name) 
                        ?? "admin@skillhub.com";

        try
        {
            await using var stream = file.OpenReadStream();
            var result = await enrollmentService.ImportFromExcelAsync(stream, userEmail, cancellationToken);
            return View("ImportResult", result);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error al procesar el archivo: {ex.Message}");
            return View();
        }
    }

    [HttpPost]
    [Authorize(Roles = UserRoles.Employee)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClearAll(CancellationToken cancellationToken)
    {
        try
        {
            var count = await enrollmentService.ClearAllEnrollmentsAsync(cancellationToken);
            TempData["Success"] = $"¡Se han limpiado {count} inscripciones exitosamente! Los cupos fueron restablecidos para una nueva importación masiva.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al limpiar las inscripciones: {ex.Message}";
        }

        return RedirectToAction(nameof(Import));
    }
}
