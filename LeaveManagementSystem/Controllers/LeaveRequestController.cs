using Microsoft.AspNetCore.Mvc;
using LeaveManagementSystem.Data;
using LeaveManagementSystem.DTOs;
using LeaveManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagementSystem.Controllers;

[Route("api/leaves")]
[ApiController]
public class LeaveRequestController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public LeaveRequestController(ApplicationDbContext context)
    {
        _context = context;
    }


    // POST: api/leaves
    // Apply for leave
    [HttpPost]
    public async Task<ActionResult<LeaveRequestDto>> CreateLeaveRequest(
        CreateLeaveRequestDto leaveRequestDto)
    {
        // 1. Check employee exists
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e =>
                e.EmployeeId == leaveRequestDto.EmployeeId);

        if (employee == null)
        {
            return NotFound("Employee not found.");
        }

        // 2. Check employee is active
        if (!employee.IsActive)
        {
            return BadRequest("Employee is not active.");
        }


        // 3. Check leave type exists
        var leaveType = await _context.LeaveTypes
            .FirstOrDefaultAsync(l =>
                l.LeaveTypeId == leaveRequestDto.LeaveTypeId);

        if (leaveType == null)
        {
            return NotFound("Leave type not found.");
        }

        // 4. Check leave type is active
        if (!leaveType.IsActive)
        {
            return BadRequest("Leave type is not active.");
        }


        // 5. Validate dates
        if (leaveRequestDto.FromDate > leaveRequestDto.ToDate)
        {
            return BadRequest("FromDate cannot be after ToDate.");
        }


        // 6. Calculate number of days internally
        var numberOfDays =
            (leaveRequestDto.ToDate - leaveRequestDto.FromDate).Days + 1;


        // 7. Find employee's leave balance
        var leaveBalance = await _context.LeaveBalances
            .FirstOrDefaultAsync(b =>
                b.EmployeeId == leaveRequestDto.EmployeeId &&
                b.LeaveTypeId == leaveRequestDto.LeaveTypeId);

        if (leaveBalance == null)
        {
            return BadRequest(
                "Leave balance not found for this employee and leave type.");
        }


        // 8. Calculate remaining balance
        var remainingDays =
            leaveBalance.TotalDays - leaveBalance.UsedDays;


        // 9. Check sufficient balance
        if (numberOfDays > remainingDays)
        {
            return BadRequest(
                $"Insufficient leave balance. Remaining days: {remainingDays}.");
        }


        // 10. Check overlapping leave
        var overlappingLeave = await _context.LeaveRequests
            .AnyAsync(l =>
                l.EmployeeId == leaveRequestDto.EmployeeId &&
                l.Status != "Rejected" &&
                leaveRequestDto.FromDate <= l.ToDate &&
                leaveRequestDto.ToDate >= l.FromDate);

        if (overlappingLeave)
        {
            return BadRequest(
                "Leave request overlaps with an existing leave period.");
        }


        // 11. Create leave request
        // Status and AppliedDate are controlled by server
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


        // 12. Prepare response
        var response = new LeaveRequestDto
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


        return CreatedAtAction(
            nameof(GetLeaveRequest),
            new { id = leaveRequest.LeaveRequestId },
            response);
    }


    // GET: api/leaves
    // Supports optional filters
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LeaveRequestDto>>> GetLeaveRequests(
        int? employeeId,
        int? leaveTypeId,
        string? status,
        DateTime? fromDate,
        DateTime? toDate)
    {
        var query = _context.LeaveRequests
            .AsQueryable();


        // Filter by employee
        if (employeeId.HasValue)
        {
            query = query.Where(l =>
                l.EmployeeId == employeeId.Value);
        }


        // Filter by leave type
        if (leaveTypeId.HasValue)
        {
            query = query.Where(l =>
                l.LeaveTypeId == leaveTypeId.Value);
        }


        // Filter by status
        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(l =>
                l.Status == status);
        }


        // Filter by FromDate
        if (fromDate.HasValue)
        {
            query = query.Where(l =>
                l.FromDate >= fromDate.Value);
        }


        // Filter by ToDate
        if (toDate.HasValue)
        {
            query = query.Where(l =>
                l.ToDate <= toDate.Value);
        }


        var leaveRequests = await query
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


        return Ok(leaveRequests);
    }


    // GET: api/leaves/1
    [HttpGet("{id}")]
    public async Task<ActionResult<LeaveRequestDto>> GetLeaveRequest(int id)
    {
        var leaveRequest = await _context.LeaveRequests
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


        if (leaveRequest == null)
        {
            return NotFound();
        }


        return Ok(leaveRequest);
    }


    // GET: api/employees/1/leaves
    // Employee leave history
    [HttpGet("/api/employees/{employeeId}/leaves")]
    public async Task<ActionResult<IEnumerable<LeaveRequestDto>>> GetEmployeeLeaves(
        int employeeId)
    {
        var employeeExists = await _context.Employees
            .AnyAsync(e => e.EmployeeId == employeeId);

        if (!employeeExists)
        {
            return NotFound("Employee not found.");
        }


        var leaveRequests = await _context.LeaveRequests
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


        return Ok(leaveRequests);
    }


    // PUT: api/leaves/1/approve
    [HttpPut("{id}/approve")]
    public async Task<IActionResult> ApproveLeaveRequest(
        int id,
        LeaveActionDto leaveActionDto)
    {
        // Find request
        var leaveRequest = await _context.LeaveRequests
            .FindAsync(id);

        if (leaveRequest == null)
        {
            return NotFound("Leave request not found.");
        }


        // Only Pending requests can be approved
        if (leaveRequest.Status != "Pending")
        {
            return BadRequest(
                "Only Pending leave requests can be approved.");
        }


        // Calculate approved leave days
        var numberOfDays =
            (leaveRequest.ToDate - leaveRequest.FromDate).Days + 1;


        // Find balance
        var leaveBalance = await _context.LeaveBalances
            .FirstOrDefaultAsync(b =>
                b.EmployeeId == leaveRequest.EmployeeId &&
                b.LeaveTypeId == leaveRequest.LeaveTypeId);

        if (leaveBalance == null)
        {
            return BadRequest("Leave balance not found.");
        }


        // Check balance again before approval
        var remainingDays =
            leaveBalance.TotalDays - leaveBalance.UsedDays;

        if (numberOfDays > remainingDays)
        {
            return BadRequest(
                $"Insufficient leave balance. Remaining days: {remainingDays}.");
        }


        // Approve request
        leaveRequest.Status = "Approved";

        leaveRequest.ApprovedBy =
            leaveActionDto.ApprovedBy;

        leaveRequest.Comments =
            leaveActionDto.Comments;


        // Update used leave
        leaveBalance.UsedDays += numberOfDays;


        await _context.SaveChangesAsync();


        return NoContent();
    }


    // PUT: api/leaves/1/reject
    [HttpPut("{id}/reject")]
    public async Task<IActionResult> RejectLeaveRequest(
        int id,
        LeaveActionDto leaveActionDto)
    {
        var leaveRequest = await _context.LeaveRequests
            .FindAsync(id);

        if (leaveRequest == null)
        {
            return NotFound("Leave request not found.");
        }


        // Only Pending requests can be rejected
        if (leaveRequest.Status != "Pending")
        {
            return BadRequest(
                "Only Pending leave requests can be rejected.");
        }


        // Reject request
        leaveRequest.Status = "Rejected";

        leaveRequest.ApprovedBy =
            leaveActionDto.ApprovedBy;

        leaveRequest.Comments =
            leaveActionDto.Comments;


        // IMPORTANT:
        // Do NOT update LeaveBalance for rejected requests


        await _context.SaveChangesAsync();


        return NoContent();
    }
}