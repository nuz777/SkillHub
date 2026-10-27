using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillHub.Application.DTOs;
using SkillHub.Application.Interfaces;
using SkillHub.Domain.Enums;

namespace SkillHub.Api.Controllers;

[ApiController]
[Route("api/v1/reports")]
[Authorize(Roles = UserRoles.Employee)]
public class ReportsController(IWorkshopService workshopService) : ControllerBase
{
    [HttpGet("top-workshops")]
    [ProducesResponseType(typeof(IEnumerable<TopWorkshopReportDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TopWorkshopReportDto>>> GetTopWorkshops(CancellationToken cancellationToken)
    {
        var report = await workshopService.GetTopWorkshopsReportAsync(cancellationToken);
        return Ok(report);
    }
}
