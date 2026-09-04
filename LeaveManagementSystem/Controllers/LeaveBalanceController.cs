using Microsoft.AspNetCore.Mvc;
using LeaveManagementSystem.Data;
using LeaveManagementSystem.DTOs;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagementSystem.Controllers;

[Route("api/employees")]
[ApiController]
public class LeaveBalanceController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public LeaveBalanceController(ApplicationDbContext context)
    {
        _context = context;
    }


    // GET: api/employees/1/leave-balance
    [HttpGet("{employeeId}/leave-balance")]
    public async Task<ActionResult<IEnumerable<LeaveBalanceDto>>> GetLeaveBalance(
        int employeeId)
    {
        // Check employee exists
        var employeeExists = await _context.Employees
            .AnyAsync(e => e.EmployeeId == employeeId);

        if (!employeeExists)
        {
            return NotFound("Employee not found.");
        }


        var balances = await _context.LeaveBalances
            .Where(b => b.EmployeeId == employeeId)
            .Select(b => new LeaveBalanceDto
            {
                LeaveType = b.LeaveType.LeaveTypeName,
                TotalDays = b.TotalDays,
                UsedDays = b.UsedDays,
                RemainingDays = b.TotalDays - b.UsedDays
            })
            .ToListAsync();


        return Ok(balances);
    }
}