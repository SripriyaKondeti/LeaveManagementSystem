using LeaveManagementSystem.Data;
using LeaveManagementSystem.DTOs;
using LeaveManagementSystem.Middleware;
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
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

        if (employee == null)
        {
            return null;
        }

        // Check employee is active
        if (!employee.IsActive)
        {
            throw new ConflictException(
                $"Employee with ID {employeeId} is inactive.");
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