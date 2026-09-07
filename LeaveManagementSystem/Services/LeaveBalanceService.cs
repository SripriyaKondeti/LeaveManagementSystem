using LeaveManagementSystem.Data;
using LeaveManagementSystem.DTOs;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagementSystem.Services;

public class LeaveBalanceService
{
    private readonly ApplicationDbContext _context;

    public LeaveBalanceService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<LeaveBalanceDto>?> GetLeaveBalanceAsync(
        int employeeId)
    {
        // Check employee exists
        var employeeExists = await _context.Employees
            .AnyAsync(e => e.EmployeeId == employeeId);

        if (!employeeExists)
        {
            return null;
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

        return balances;
    }
}