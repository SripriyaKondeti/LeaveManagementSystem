using LeaveManagementSystem.Data;
using LeaveManagementSystem.DTOs;
using LeaveManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagementSystem.Services;

public class LeaveRequestService
{
    private readonly ApplicationDbContext _context;

    public LeaveRequestService(ApplicationDbContext context)
    {
        _context = context;
    }

    // Apply for leave
    public async Task<LeaveRequestDto?> CreateLeaveRequestAsync(
        CreateLeaveRequestDto leaveRequestDto)
    {
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e =>
                e.EmployeeId == leaveRequestDto.EmployeeId);

        if (employee == null || !employee.IsActive)
        {
            return null;
        }

        var leaveType = await _context.LeaveTypes
            .FirstOrDefaultAsync(l =>
                l.LeaveTypeId == leaveRequestDto.LeaveTypeId);

        if (leaveType == null || !leaveType.IsActive)
        {
            return null;
        }

        if (leaveRequestDto.FromDate > leaveRequestDto.ToDate)
        {
            return null;
        }

        var numberOfDays =
            (leaveRequestDto.ToDate - leaveRequestDto.FromDate).Days + 1;

        var leaveBalance = await _context.LeaveBalances
            .FirstOrDefaultAsync(b =>
                b.EmployeeId == leaveRequestDto.EmployeeId &&
                b.LeaveTypeId == leaveRequestDto.LeaveTypeId);

        if (leaveBalance == null)
        {
            return null;
        }

        var remainingDays =
            leaveBalance.TotalDays - leaveBalance.UsedDays;

        if (numberOfDays > remainingDays)
        {
            return null;
        }

        var overlappingLeave = await _context.LeaveRequests
            .AnyAsync(l =>
                l.EmployeeId == leaveRequestDto.EmployeeId &&
                l.Status != "Rejected" &&
                leaveRequestDto.FromDate <= l.ToDate &&
                leaveRequestDto.ToDate >= l.FromDate);

        if (overlappingLeave)
        {
            return null;
        }

        var leaveRequest = new LeaveRequest
        {
            EmployeeId = leaveRequestDto.EmployeeId,
            LeaveTypeId = leaveRequestDto.LeaveTypeId,
            FromDate = leaveRequestDto.FromDate,
            ToDate = leaveRequestDto.ToDate,
            Reason = leaveRequestDto.Reason,
            Status = "Pending",
            AppliedDate = DateTime.Now,
            ApprovedBy = null,
            Comments = null
        };

        _context.LeaveRequests.Add(leaveRequest);

        await _context.SaveChangesAsync();

        return new LeaveRequestDto
        {
            LeaveRequestId = leaveRequest.LeaveRequestId,
            EmployeeId = leaveRequest.EmployeeId,
            LeaveTypeId = leaveRequest.LeaveTypeId,
            FromDate = leaveRequest.FromDate,
            ToDate = leaveRequest.ToDate,
            Reason = leaveRequest.Reason,
            Status = leaveRequest.Status,
            AppliedDate = leaveRequest.AppliedDate,
            ApprovedBy = leaveRequest.ApprovedBy,
            Comments = leaveRequest.Comments
        };
    }

    // Get all leave requests with filters
    public async Task<IEnumerable<LeaveRequestDto>> GetLeaveRequestsAsync(
        int? employeeId,
        int? leaveTypeId,
        string? status,
        DateTime? fromDate,
        DateTime? toDate)
    {
        var query = _context.LeaveRequests
            .AsQueryable();

        if (employeeId.HasValue)
        {
            query = query.Where(l =>
                l.EmployeeId == employeeId.Value);
        }

        if (leaveTypeId.HasValue)
        {
            query = query.Where(l =>
                l.LeaveTypeId == leaveTypeId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(l =>
                l.Status == status);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(l =>
                l.FromDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(l =>
                l.ToDate <= toDate.Value);
        }

        return await query
            .Select(l => new LeaveRequestDto
            {
                LeaveRequestId = l.LeaveRequestId,
                EmployeeId = l.EmployeeId,
                LeaveTypeId = l.LeaveTypeId,
                FromDate = l.FromDate,
                ToDate = l.ToDate,
                Reason = l.Reason,
                Status = l.Status,
                AppliedDate = l.AppliedDate,
                ApprovedBy = l.ApprovedBy,
                Comments = l.Comments
            })
            .ToListAsync();
    }

    // Get one leave request
    public async Task<LeaveRequestDto?> GetLeaveRequestAsync(int id)
    {
        return await _context.LeaveRequests
            .Where(l => l.LeaveRequestId == id)
            .Select(l => new LeaveRequestDto
            {
                LeaveRequestId = l.LeaveRequestId,
                EmployeeId = l.EmployeeId,
                LeaveTypeId = l.LeaveTypeId,
                FromDate = l.FromDate,
                ToDate = l.ToDate,
                Reason = l.Reason,
                Status = l.Status,
                AppliedDate = l.AppliedDate,
                ApprovedBy = l.ApprovedBy,
                Comments = l.Comments
            })
            .FirstOrDefaultAsync();
    }

    // Employee leave history
    public async Task<IEnumerable<LeaveRequestDto>?> GetEmployeeLeavesAsync(
        int employeeId)
    {
        var employeeExists = await _context.Employees
            .AnyAsync(e => e.EmployeeId == employeeId);

        if (!employeeExists)
        {
            return null;
        }

        return await _context.LeaveRequests
            .Where(l => l.EmployeeId == employeeId)
            .Select(l => new LeaveRequestDto
            {
                LeaveRequestId = l.LeaveRequestId,
                EmployeeId = l.EmployeeId,
                LeaveTypeId = l.LeaveTypeId,
                FromDate = l.FromDate,
                ToDate = l.ToDate,
                Reason = l.Reason,
                Status = l.Status,
                AppliedDate = l.AppliedDate,
                ApprovedBy = l.ApprovedBy,
                Comments = l.Comments
            })
            .ToListAsync();
    }

    // Approve leave request
    public async Task<bool> ApproveLeaveRequestAsync(
        int id,
        LeaveActionDto leaveActionDto)
    {
        var leaveRequest = await _context.LeaveRequests
            .FindAsync(id);

        if (leaveRequest == null)
        {
            return false;
        }

        if (leaveRequest.Status != "Pending")
        {
            return false;
        }

        var numberOfDays =
            (leaveRequest.ToDate - leaveRequest.FromDate).Days + 1;

        var leaveBalance = await _context.LeaveBalances
            .FirstOrDefaultAsync(b =>
                b.EmployeeId == leaveRequest.EmployeeId &&
                b.LeaveTypeId == leaveRequest.LeaveTypeId);

        if (leaveBalance == null)
        {
            return false;
        }

        var remainingDays =
            leaveBalance.TotalDays - leaveBalance.UsedDays;

        if (numberOfDays > remainingDays)
        {
            return false;
        }

        leaveRequest.Status = "Approved";

        leaveRequest.ApprovedBy =
            leaveActionDto.ApprovedBy;

        leaveRequest.Comments =
            leaveActionDto.Comments;

        leaveBalance.UsedDays += numberOfDays;

        await _context.SaveChangesAsync();

        return true;
    }

    // Reject leave request
    public async Task<bool> RejectLeaveRequestAsync(
        int id,
        LeaveActionDto leaveActionDto)
    {
        var leaveRequest = await _context.LeaveRequests
            .FindAsync(id);

        if (leaveRequest == null)
        {
            return false;
        }

        if (leaveRequest.Status != "Pending")
        {
            return false;
        }

        leaveRequest.Status = "Rejected";

        leaveRequest.ApprovedBy =
            leaveActionDto.ApprovedBy;

        leaveRequest.Comments =
            leaveActionDto.Comments;

        // Do not update balance for rejected leave

        await _context.SaveChangesAsync();

        return true;
    }
}