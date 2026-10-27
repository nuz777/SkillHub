using Microsoft.AspNetCore.Mvc;
using SkillHub.Application.Interfaces;

namespace SkillHub.Web.Controllers;

public class WorkshopsController(IWorkshopService workshopService) : Controller
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
}
