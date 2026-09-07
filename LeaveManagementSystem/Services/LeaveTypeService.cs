using LeaveManagementSystem.Data;
using LeaveManagementSystem.DTOs;
using LeaveManagementSystem.Models;
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
        var leaveType = new LeaveType
        {
            LeaveTypeName = leaveTypeDto.LeaveTypeName,
            MaximumDays = leaveTypeDto.MaximumDays,
            IsActive = leaveTypeDto.IsActive
        };

        _context.LeaveTypes.Add(leaveType);

        await _context.SaveChangesAsync();

        leaveTypeDto.LeaveTypeId = leaveType.LeaveTypeId;

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

        leaveType.LeaveTypeName = leaveTypeDto.LeaveTypeName;
        leaveType.MaximumDays = leaveTypeDto.MaximumDays;
        leaveType.IsActive = leaveTypeDto.IsActive;

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