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
    public async Task<ActionResult<IEnumerable<LeaveBalanceDto>>> GetLeaveBalance(
    int employeeId)
    {
        if (employeeId <= 0)
        {
            return BadRequest(new { statusCode = 400, message = "Employee ID must be greater than zero." });
        }

        if (!User.IsInRole("Manager"))
        {
            var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;

            if (!int.TryParse(employeeIdClaim, out var loggedInEmployeeId))
            {
                return StatusCode(
                    StatusCodes.Status401Unauthorized,
                    new { statusCode = 401, message = "Employee information is missing from the token." });
            }

            // Employees can view only their own balance
            if (loggedInEmployeeId != employeeId)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    "You don't have access to this employee's leave balance.");
            }
        }

        var balances = await _leaveBalanceService.GetLeaveBalanceAsync(employeeId);

        if (balances == null)
        {
            return NotFound(new { statusCode = 404, message = $"Employee with ID {employeeId} does not exist." });
        }

        return Ok(balances);
    }
}