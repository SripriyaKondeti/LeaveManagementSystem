using LeaveManagementSystem.Data;
using LeaveManagementSystem.DTOs;
using LeaveManagementSystem.Models;
using LeaveManagementSystem.Middleware;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagementSystem.Services;

public class LeaveTypeService
{
    private readonly ApplicationDbContext _context;

    public LeaveTypeService(ApplicationDbContext context)
    {
        _context = context;
    }

    // Get all leave types
    public async Task<IEnumerable<LeaveTypeDto>> GetLeaveTypesAsync()
    {
        return await _context.LeaveTypes
            .Select(l => new LeaveTypeDto
            {
                LeaveTypeId = l.LeaveTypeId,
                LeaveTypeName = l.LeaveTypeName,
                MaximumDays = l.MaximumDays,
                IsActive = l.IsActive
            })
            .ToListAsync();
    }

    // Get one leave type
    public async Task<LeaveTypeDto?> GetLeaveTypeAsync(int id)
    {
        return await _context.LeaveTypes
            .Where(l => l.LeaveTypeId == id)
            .Select(l => new LeaveTypeDto
            {
                LeaveTypeId = l.LeaveTypeId,
                LeaveTypeName = l.LeaveTypeName,
                MaximumDays = l.MaximumDays,
                IsActive = l.IsActive
            })
            .FirstOrDefaultAsync();
    }

    // Create leave type
    public async Task<LeaveTypeDto> CreateLeaveTypeAsync(
     LeaveTypeDto leaveTypeDto)
    {
        // Validate leave type name
        if (string.IsNullOrWhiteSpace(leaveTypeDto.LeaveTypeName))
        {
            throw new InvalidOperationException(
                "Leave type name cannot be empty.");
        }

        // Validate maximum days
        if (leaveTypeDto.MaximumDays <= 0)
        {
            throw new InvalidOperationException(
                "Maximum days must be greater than 0.");
        }

        // Check for duplicate leave type name
        var nameExists = await _context.LeaveTypes
            .AnyAsync(l =>
                l.LeaveTypeName.ToLower() ==
                leaveTypeDto.LeaveTypeName.Trim().ToLower());

        if (nameExists)
        {
            throw new ConflictException(
                $"Leave type '{leaveTypeDto.LeaveTypeName.Trim()}' already exists.");
        }

        var leaveType = new LeaveType
        {
            LeaveTypeName = leaveTypeDto.LeaveTypeName.Trim(),
            MaximumDays = leaveTypeDto.MaximumDays,
            IsActive = leaveTypeDto.IsActive
        };

        _context.LeaveTypes.Add(leaveType);

        await _context.SaveChangesAsync();

        // Create leave balances for all active employees
        if (leaveType.IsActive)
        {
            var activeEmployees = await _context.Employees
                .Where(e => e.IsActive)
                .ToListAsync();

            var leaveBalances = activeEmployees
                .Select(employee => new LeaveBalance
                {
                    EmployeeId = employee.EmployeeId,
                    LeaveTypeId = leaveType.LeaveTypeId,
                    TotalDays = leaveType.MaximumDays,
                    UsedDays = 0
                })
                .ToList();

            _context.LeaveBalances.AddRange(leaveBalances);

            await _context.SaveChangesAsync();
        }

        leaveTypeDto.LeaveTypeId = leaveType.LeaveTypeId;
        leaveTypeDto.LeaveTypeName = leaveType.LeaveTypeName;

        return leaveTypeDto;
    }

    // Update leave type
    public async Task<bool> UpdateLeaveTypeAsync(
        int id,
        LeaveTypeDto leaveTypeDto)
    {
        var leaveType = await _context.LeaveTypes.FindAsync(id);

        if (leaveType == null)
        {
            return false;
        }

        // Validate leave type name
        if (string.IsNullOrWhiteSpace(leaveTypeDto.LeaveTypeName))
        {
            throw new InvalidOperationException(
                "Leave type name cannot be empty.");
        }

        // Validate maximum days
        if (leaveTypeDto.MaximumDays <= 0)
        {
            throw new InvalidOperationException(
                "Maximum days must be greater than 0.");
        }

        // Check for duplicate leave type name
        var nameExists = await _context.LeaveTypes
            .AnyAsync(l =>
                l.LeaveTypeId != id &&
                l.LeaveTypeName.ToLower() ==
                leaveTypeDto.LeaveTypeName.Trim().ToLower());

        if (nameExists)
        {
            throw new ConflictException(
                $"Leave type '{leaveTypeDto.LeaveTypeName.Trim()}' already exists.");
        }

        var wasInactive = !leaveType.IsActive;

        leaveType.LeaveTypeName = leaveTypeDto.LeaveTypeName.Trim();
        leaveType.MaximumDays = leaveTypeDto.MaximumDays;
        leaveType.IsActive = leaveTypeDto.IsActive;

        if (wasInactive && leaveType.IsActive)
        {
            var activeEmployeeIds = await _context.Employees
                .Where(e => e.IsActive)
                .Select(e => e.EmployeeId)
                .ToListAsync();

            var existingBalanceEmployeeIds = await _context.LeaveBalances
                .Where(b => b.LeaveTypeId == id)
                .Select(b => b.EmployeeId)
                .ToListAsync();

            var missingBalances = activeEmployeeIds
                .Except(existingBalanceEmployeeIds)
                .Select(employeeId => new LeaveBalance
                {
                    EmployeeId = employeeId,
                    LeaveTypeId = id,
                    TotalDays = leaveType.MaximumDays,
                    UsedDays = 0
                })
                .ToList();

            _context.LeaveBalances.AddRange(missingBalances);
        }

        await _context.SaveChangesAsync();

        return true;
    }

    // Deactivate leave type
    public async Task<bool> DeactivateLeaveTypeAsync(int id)
    {
        var leaveType = await _context.LeaveTypes.FindAsync(id);

        if (leaveType == null)
        {
            return false;
        }

        leaveType.IsActive = false;

        await _context.SaveChangesAsync();

        return true;
    }
}