using Microsoft.AspNetCore.Mvc;
using LeaveManagementSystem.DTOs;
using LeaveManagementSystem.Services;

namespace LeaveManagementSystem.Controllers;

[Route("api/employees")]
[ApiController]
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
        var balances = await _leaveBalanceService
            .GetLeaveBalanceAsync(employeeId);

        if (balances == null)
        {
            return NotFound("Employee not found.");
        }

        return Ok(balances);
    }
}