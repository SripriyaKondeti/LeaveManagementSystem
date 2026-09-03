using Microsoft.AspNetCore.Mvc;
using LeaveManagementSystem.Data;
using LeaveManagementSystem.DTOs;
using LeaveManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagementSystem.Controllers;

[Route("api/leave-requests")]
[ApiController]
public class LeaveRequestController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public LeaveRequestController(ApplicationDbContext context)
    {
        _context = context;
    }


    // GET: api/leave-requests
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LeaveRequestDto>>> GetLeaveRequests()
    {
        var leaveRequests = await _context.LeaveRequests
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


    // GET: api/leave-requests/1
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


    // POST: api/leave-requests
    [HttpPost]
    public async Task<ActionResult<LeaveRequestDto>> CreateLeaveRequest(
        LeaveRequestDto leaveRequestDto)
    {
        // Calculate number of leave days internally
        var numberOfDays =
            (leaveRequestDto.ToDate - leaveRequestDto.FromDate).Days + 1;

        // Create LeaveRequest entity
        var leaveRequest = new LeaveRequest
        {
            EmployeeId = leaveRequestDto.EmployeeId,
            LeaveTypeId = leaveRequestDto.LeaveTypeId,
            FromDate = leaveRequestDto.FromDate,
            ToDate = leaveRequestDto.ToDate,
            Reason = leaveRequestDto.Reason,

            // New requests always start as Pending
            Status = "Pending",

            // Set automatically by system
            AppliedDate = DateTime.Now,

            ApprovedBy = null,
            Comments = leaveRequestDto.Comments
        };

        _context.LeaveRequests.Add(leaveRequest);

        await _context.SaveChangesAsync();

        leaveRequestDto.LeaveRequestId = leaveRequest.LeaveRequestId;
        leaveRequestDto.Status = leaveRequest.Status;
        leaveRequestDto.AppliedDate = leaveRequest.AppliedDate;

        return CreatedAtAction(
            nameof(GetLeaveRequest),
            new { id = leaveRequest.LeaveRequestId },
            leaveRequestDto
        );
    }
}