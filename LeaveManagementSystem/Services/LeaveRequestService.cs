using LeaveManagementSystem.Data;
using LeaveManagementSystem.DTOs;
using LeaveManagementSystem.Models;
using LeaveManagementSystem.Middleware;
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
    public async Task<LeaveRequestDto> CreateLeaveRequestAsync(
        CreateLeaveRequestDto leaveRequestDto)
    {
        // 1. Check employee exists
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e =>
                e.EmployeeId == leaveRequestDto.EmployeeId);

        if (employee == null)
        {
            throw new KeyNotFoundException(
                $"Employee with ID {leaveRequestDto.EmployeeId} does not exist.");
        }

        // 2. Check employee is active
        if (!employee.IsActive)
        {
            throw new ConflictException(
                $"Employee with ID {leaveRequestDto.EmployeeId} is not active.");
        }

        // 3. Check leave type exists
        var leaveType = await _context.LeaveTypes
            .FirstOrDefaultAsync(l =>
                l.LeaveTypeId == leaveRequestDto.LeaveTypeId);

        if (leaveType == null)
        {
            throw new KeyNotFoundException(
                $"Leave type with ID {leaveRequestDto.LeaveTypeId} does not exist.");
        }

        // 4. Check leave type is active
        if (!leaveType.IsActive)
        {
            throw new InvalidOperationException(
                "The selected leave type is not active.");
        }

        // 5. Validate dates
        if (leaveRequestDto.FromDate > leaveRequestDto.ToDate)
        {
            throw new InvalidOperationException(
                "From date cannot be after To date.");
        }

            if (leaveRequestDto.FromDate.Date < DateTime.Today)
            {
                throw new InvalidOperationException(
                "Leave cannot start before today.");
            }

        // 6. Calculate number of days internally
        var numberOfDays =
            (leaveRequestDto.ToDate - leaveRequestDto.FromDate).Days + 1;

        // 7. Check leave balance
        var leaveBalance = await _context.LeaveBalances
            .FirstOrDefaultAsync(b =>
                b.EmployeeId == leaveRequestDto.EmployeeId &&
                b.LeaveTypeId == leaveRequestDto.LeaveTypeId);

        if (leaveBalance == null)
        {
            throw new KeyNotFoundException(
                "Leave balance was not found for this employee and leave type.");
        }

        // 8. Calculate remaining days
        var remainingDays =
            leaveBalance.TotalDays - leaveBalance.UsedDays;

        // 9. Check sufficient balance
        if (numberOfDays > remainingDays)
        {
            throw new InvalidOperationException(
                $"Insufficient leave balance. Remaining days: {remainingDays}.");
        }

        // 10. Check overlapping leave
        var overlappingLeave = await _context.LeaveRequests
            .AnyAsync(l =>
                l.EmployeeId == leaveRequestDto.EmployeeId &&
                (l.Status == "Pending" || l.Status == "Approved") &&
                leaveRequestDto.FromDate <= l.ToDate &&
                leaveRequestDto.ToDate >= l.FromDate);

        if (overlappingLeave)
        {
            throw new ConflictException(
                "The requested leave dates overlap with an existing leave request.");
        }

        // 11. Create leave request
        var leaveRequest = new LeaveRequest
        {
            EmployeeId = leaveRequestDto.EmployeeId,
            LeaveTypeId = leaveRequestDto.LeaveTypeId,
            FromDate = leaveRequestDto.FromDate,
            ToDate = leaveRequestDto.ToDate,
            Reason = leaveRequestDto.Reason,

            // Controlled by server
            Status = "Pending",
            AppliedDate = DateTime.Now,

            ApprovedBy = null,
            Comments = null
        };

        _context.LeaveRequests.Add(leaveRequest);

        await _context.SaveChangesAsync();

        // 12. Return response
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
        // Check employee exists
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
    public async Task<LeaveRequestDto> ApproveLeaveRequestAsync(
        int id,
        LeaveActionDto leaveActionDto)
    {
        // 1. Find leave request
        var leaveRequest = await _context.LeaveRequests
            .FindAsync(id);

        if (leaveRequest == null)
        {
            throw new KeyNotFoundException(
                $"Leave request with ID {id} does not exist.");
        }

        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.EmployeeId == leaveRequest.EmployeeId);

        if (employee == null)
        {
            throw new KeyNotFoundException(
                $"Employee with ID {leaveRequest.EmployeeId} does not exist.");
        }

        if (!employee.IsActive)
        {
            throw new ConflictException(
                $"Cannot approve leave for inactive employee with ID {employee.EmployeeId}.");
        }

        // 2. Check current status
        if (leaveRequest.Status == "Approved")
        {
            throw new ConflictException(
                "Leave request is already approved.");
        }

        if (leaveRequest.Status == "Rejected")
        {
            throw new ConflictException(
                "Leave request is already rejected.");
        }

        // 3. Calculate number of days
        var numberOfDays =
            (leaveRequest.ToDate - leaveRequest.FromDate).Days + 1;

        // 4. Find leave balance
        var leaveBalance = await _context.LeaveBalances
            .FirstOrDefaultAsync(b =>
                b.EmployeeId == leaveRequest.EmployeeId &&
                b.LeaveTypeId == leaveRequest.LeaveTypeId);

        if (leaveBalance == null)
        {
            throw new KeyNotFoundException(
                "Leave balance was not found for this employee and leave type.");
        }

        // 5. Calculate remaining days
        var remainingDays =
            leaveBalance.TotalDays - leaveBalance.UsedDays;

        // 6. Check sufficient balance
        if (numberOfDays > remainingDays)
        {
            throw new InvalidOperationException(
                $"Insufficient leave balance. Remaining days: {remainingDays}.");
        }

        // 7. Approve request
        leaveRequest.Status = "Approved";

        leaveRequest.ApprovedBy =
            leaveActionDto.ApprovedBy;

        leaveRequest.Comments =
            leaveActionDto.Comments;

        // 8. Update balance only after approval
        leaveBalance.UsedDays += numberOfDays;

        await _context.SaveChangesAsync();

        return (await GetLeaveRequestAsync(id))!;
    }


    // Reject leave request
    public async Task<LeaveRequestDto> RejectLeaveRequestAsync(
        int id,
        LeaveActionDto leaveActionDto)
    {
        // 1. Find leave request
        var leaveRequest = await _context.LeaveRequests
            .FindAsync(id);

        if (leaveRequest == null)
        {
            throw new KeyNotFoundException(
                $"Leave request with ID {id} does not exist.");
        }

        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.EmployeeId == leaveRequest.EmployeeId);

        if (employee == null)
        {
            throw new KeyNotFoundException(
                $"Employee with ID {leaveRequest.EmployeeId} does not exist.");
        }

        if (!employee.IsActive)
        {
            throw new ConflictException(
                $"Cannot reject leave for inactive employee with ID {employee.EmployeeId}.");
        }

        // 2. Check current status
        if (leaveRequest.Status == "Rejected")
        {
            throw new ConflictException(
                "Leave request is already rejected.");
        }

        if (leaveRequest.Status == "Approved")
        {
            throw new ConflictException(
                "Leave request is already approved.");
        }

        // 3. Reject request
        leaveRequest.Status = "Rejected";

        leaveRequest.ApprovedBy =
            leaveActionDto.ApprovedBy;

        leaveRequest.Comments =
            leaveActionDto.Comments;

        // Balance is NOT updated for rejected leave

        await _context.SaveChangesAsync();

        return (await GetLeaveRequestAsync(id))!;
    }
}