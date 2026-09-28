using System.Security.Claims;
using LeaveManagementSystem.DTOs;
using LeaveManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeaveManagementSystem.Controllers;

[Route("api/employees")]
[ApiController]
[Authorize]
public class LeaveBalanceController : ControllerBase
{
    private readonly LeaveBalanceService _leaveBalanceService;

    public LeaveBalanceController(
        LeaveBalanceService leaveBalanceService)
    {
        _leaveBalanceService = leaveBalanceService;
    }

    [HttpGet("{employeeId}/leave-balance")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<LeaveBalanceDto>>>> GetLeaveBalance(
    int employeeId)
    {
        if (employeeId <= 0)
        {
            return BadRequest(ApiResponseDto<object>.Failure(400, "Employee ID must be greater than zero."));
        }

        if (!User.IsInRole("Manager"))
        {
            var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;

            if (!int.TryParse(employeeIdClaim, out var loggedInEmployeeId))
            {
                return StatusCode(
                    StatusCodes.Status401Unauthorized,
                    ApiResponseDto<object>.Failure(401, "Employee information is missing from the token."));
            }

            // Employees can view only their own balance
            if (loggedInEmployeeId != employeeId)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponseDto<object>.Failure(403, "You don't have access to this employee's leave balance."));
            }
        }

        var balances = await _leaveBalanceService.GetLeaveBalanceAsync(employeeId);

        if (balances == null)
        {
            return NotFound(ApiResponseDto<object>.Failure(404, $"Employee with ID {employeeId} does not exist."));
        }

        return Ok(ApiResponseDto<IEnumerable<LeaveBalanceDto>>.Success(200, "Leave balance retrieved successfully.", balances));
    }
}
