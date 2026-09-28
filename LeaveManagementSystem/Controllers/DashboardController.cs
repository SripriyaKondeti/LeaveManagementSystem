using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LeaveManagementSystem.DTOs;
using LeaveManagementSystem.Services;

namespace LeaveManagementSystem.Controllers;

[Route("api/dashboard")]
[ApiController]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly DashboardService _dashboardService;

    public DashboardController(DashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    // GET: api/dashboard/leave-summary
    // Manager only
    [HttpGet("leave-summary")]
    public async Task<ActionResult<ApiResponseDto<LeaveSummaryDto>>> GetLeaveSummary()
    {
        if (!User.IsInRole("Manager"))
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                ApiResponseDto<object>.Failure(403, "Only managers can access the dashboard."));
        }

        var summary = await _dashboardService
            .GetLeaveSummaryAsync();

        return Ok(ApiResponseDto<LeaveSummaryDto>.Success(200, "Leave summary retrieved successfully.", summary));
    }
}
