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

    // GET: api/employees/1/leave-balance
    [HttpGet("{employeeId}/leave-balance")]
    public async Task<ActionResult<IEnumerable<LeaveBalanceDto>>> GetLeaveBalance(
        int employeeId)
    {
        // Managers can view any employee's leave balance
        if (!User.IsInRole("Manager"))
        {
            var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;

            if (!int.TryParse(employeeIdClaim, out var loggedInEmployeeId))
            {
                return Forbid();
            }

            // Employees can view only their own balance
            if (loggedInEmployeeId != employeeId)
            {
                return Forbid();
            }
        }

        var balances = await _leaveBalanceService
            .GetLeaveBalanceAsync(employeeId);

        if (balances == null)
        {
            return NotFound(
                $"Employee with ID {employeeId} does not exist.");
        }

        return Ok(balances);
    }
}