using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillHub.Application.DTOs;
using SkillHub.Application.Interfaces;
using SkillHub.Domain.Enums;
using SkillHub.Domain.Exceptions;

namespace SkillHub.Web.Controllers;

public class WorkshopsController(
    IWorkshopService workshopService,
    IEnrollmentService enrollmentService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var workshops = await workshopService.GetAvailableWorkshopsAsync(cancellationToken);
        return View(workshops);
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var workshop = await workshopService.GetByIdAsync(id, cancellationToken);
        if (workshop == null)
        {
            TempData["Error"] = "El taller solicitado no fue encontrado.";
            return RedirectToAction(nameof(Index));
        }

        return View(workshop);
    }

    [HttpGet]
    [Authorize(Roles = UserRoles.Employee)]
    public IActionResult Create()
    {
        var email = User.FindFirstValue(ClaimTypes.Email) 
                    ?? User.FindFirstValue(ClaimTypes.Name) 
                    ?? User.Identity?.Name 
                    ?? string.Empty;

        // Sugerir fecha por defecto para facilitar creación (7 días en el futuro)
        var defaultStart = DateTime.Now.Date.AddDays(7).AddHours(9);
        var defaultEnd = defaultStart.AddHours(4);

        ViewBag.DefaultStart = defaultStart.ToString("yyyy-MM-ddTHH:mm");
        ViewBag.DefaultEnd = defaultEnd.ToString("yyyy-MM-ddTHH:mm");
        ViewBag.DefaultEmail = email;

        return View();
    }

    [HttpPost]
    [Authorize(Roles = UserRoles.Employee)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateWorkshopDto dto, CancellationToken cancellationToken)
    {
        var currentEmail = User.FindFirstValue(ClaimTypes.Email) 
                           ?? User.FindFirstValue(ClaimTypes.Name) 
                           ?? User.Identity?.Name 
                           ?? string.Empty;

        // Asignar automáticamente el correo del instructor autenticado si no viene especificado
        if (string.IsNullOrWhiteSpace(dto.InstructorEmail))
        {
            dto = dto with { InstructorEmail = currentEmail };
            ModelState.Remove(nameof(dto.InstructorEmail));
        }

        ViewBag.DefaultEmail = currentEmail;
        ViewBag.DefaultStart = dto.StartDate.ToString("yyyy-MM-ddTHH:mm");
        ViewBag.DefaultEnd = dto.EndDate.ToString("yyyy-MM-ddTHH:mm");

        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        try
        {
            var created = await workshopService.CreateWorkshopAsync(dto, cancellationToken);
            TempData["Success"] = $"¡Taller '{created.Title}' ({created.Code}) publicado exitosamente!";
            return RedirectToAction(nameof(Index));
        }
        catch (BusinessRuleException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error al publicar el taller: {ex.Message}");
            return View(dto);
        }
    }

    [HttpGet]
    [Authorize(Roles = UserRoles.Employee)]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var workshop = await workshopService.GetByIdAsync(id, cancellationToken);
        if (workshop == null)
        {
            TempData["Error"] = "El taller solicitado no fue encontrado.";
            return RedirectToAction(nameof(Index));
        }

        var updateDto = new UpdateWorkshopDto(
            Title: workshop.Title,
            Description: workshop.Description,
            StartDate: workshop.StartDate,
            EndDate: workshop.EndDate,
            MaxCapacity: workshop.MaxCapacity
        );

        ViewBag.Workshop = workshop;
        ViewBag.FormattedStart = workshop.StartDate.ToString("yyyy-MM-ddTHH:mm");
        ViewBag.FormattedEnd = workshop.EndDate.ToString("yyyy-MM-ddTHH:mm");
        return View(updateDto);
    }

    [HttpPost]
    [Authorize(Roles = UserRoles.Employee)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UpdateWorkshopDto dto, CancellationToken cancellationToken)
    {
        var email = User.FindFirstValue(ClaimTypes.Email) 
                    ?? User.FindFirstValue(ClaimTypes.Name) 
                    ?? User.Identity?.Name 
                    ?? string.Empty;

        var workshop = await workshopService.GetByIdAsync(id, cancellationToken);
        ViewBag.Workshop = workshop;
        ViewBag.FormattedStart = dto.StartDate.ToString("yyyy-MM-ddTHH:mm");
        ViewBag.FormattedEnd = dto.EndDate.ToString("yyyy-MM-ddTHH:mm");

        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        try
        {
            var updated = await workshopService.UpdateWorkshopAsync(id, dto, email, true, cancellationToken);
            TempData["Success"] = $"¡Taller '{updated.Title}' actualizado exitosamente!";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (BusinessRuleException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(dto);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error al actualizar el taller: {ex.Message}");
            return View(dto);
        }
    }

    [HttpPost]
    [Authorize(Roles = UserRoles.Employee)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var email = User.FindFirstValue(ClaimTypes.Email) 
                    ?? User.FindFirstValue(ClaimTypes.Name) 
                    ?? User.Identity?.Name 
                    ?? string.Empty;

        try
        {
            await workshopService.DeleteWorkshopAsync(id, email, true, cancellationToken);
            TempData["Success"] = "El taller ha sido eliminado exitosamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (BusinessRuleException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al eliminar el taller: {ex.Message}";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    [HttpGet]
    [Authorize(Roles = UserRoles.Employee)]
    public async Task<IActionResult> Enrolled(Guid id, CancellationToken cancellationToken)
    {
        var workshop = await workshopService.GetByIdAsync(id, cancellationToken);
        if (workshop == null)
        {
            TempData["Error"] = "El taller solicitado no fue encontrado.";
            return RedirectToAction(nameof(Index));
        }

        var enrollments = await enrollmentService.GetWorkshopEnrollmentsAsync(id, cancellationToken);
        ViewBag.Workshop = workshop;
        return View(enrollments);
    }
}
