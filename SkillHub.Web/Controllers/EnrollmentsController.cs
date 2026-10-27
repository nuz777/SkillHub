using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillHub.Application.Interfaces;
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

        try
        {
            await enrollmentService.CancelEnrollmentAsync(id, userEmail, isEmployee: false, cancellationToken);
            TempData["Success"] = "Tu inscripción ha sido cancelada correctamente y el cupo ha sido liberado.";
        }
        catch (BusinessRuleException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Certificate(Guid id, CancellationToken cancellationToken)
    {
        var userEmail = User.FindFirstValue(ClaimTypes.Email) 
                        ?? User.FindFirstValue(ClaimTypes.Name) 
                        ?? string.Empty;

        try
        {
            var pdfBytes = await enrollmentService.GetCertificatePdfAsync(id, userEmail, isEmployee: false, cancellationToken);
            return File(pdfBytes, "application/pdf", $"Constancia-Inscripcion-{id}.pdf");
        }
        catch (BusinessRuleException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }
}
