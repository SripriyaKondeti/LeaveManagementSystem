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
        // First check whether the requested employee exists
        var balances = await _leaveBalanceService
            .GetLeaveBalanceAsync(employeeId);

        if (balances == null)
        {
            return NotFound(
                $"Employee with ID {employeeId} does not exist.");
        }

        // Managers can view any employee's leave balance
        if (!User.IsInRole("Manager"))
        {
            var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;

            if (!int.TryParse(employeeIdClaim, out var loggedInEmployeeId))
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    "Employee information is missing from the token.");
            }

            // Employees can view only their own balance
            if (loggedInEmployeeId != employeeId)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    "You don't have access to this employee's leave balance.");
            }
        }

        return Ok(balances);
    }
}