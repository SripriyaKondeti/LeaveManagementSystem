using Microsoft.AspNetCore.Mvc;
using LeaveManagementSystem.DTOs;
using LeaveManagementSystem.Services;

namespace LeaveManagementSystem.Controllers;

[Route("api/dashboard")]
[ApiController]
public class DashboardController : ControllerBase
{
    private readonly DashboardService _dashboardService;

    public DashboardController(DashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    // GET: api/dashboard/leave-summary
    [HttpGet("leave-summary")]
    public async Task<ActionResult<LeaveSummaryDto>> GetLeaveSummary()
    {
        var summary = await _dashboardService.GetLeaveSummaryAsync();

        return Ok(summary);
    }
}