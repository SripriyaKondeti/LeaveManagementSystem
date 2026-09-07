using Microsoft.AspNetCore.Mvc;
using LeaveManagementSystem.DTOs;
using LeaveManagementSystem.Services;

namespace LeaveManagementSystem.Controllers;

[Route("api/leaves")]
[ApiController]
public class LeaveRequestController : ControllerBase
{
    private readonly LeaveRequestService _leaveRequestService;

    public LeaveRequestController(
        LeaveRequestService leaveRequestService)
    {
        _leaveRequestService = leaveRequestService;
    }

    // POST: api/leaves
    [HttpPost]
    public async Task<ActionResult<LeaveRequestDto>> CreateLeaveRequest(
        CreateLeaveRequestDto leaveRequestDto)
    {
        var result = await _leaveRequestService
            .CreateLeaveRequestAsync(leaveRequestDto);

        if (result == null)
        {
            return BadRequest();
        }

        return CreatedAtAction(
            nameof(GetLeaveRequest),
            new { id = result.LeaveRequestId },
            result);
    }

    // GET: api/leaves
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LeaveRequestDto>>>
        GetLeaveRequests(
            int? employeeId,
            int? leaveTypeId,
            string? status,
            DateTime? fromDate,
            DateTime? toDate)
    {
        var leaveRequests = await _leaveRequestService
            .GetLeaveRequestsAsync(
                employeeId,
                leaveTypeId,
                status,
                fromDate,
                toDate);

        return Ok(leaveRequests);
    }

    // GET: api/leaves/1
    [HttpGet("{id}")]
    public async Task<ActionResult<LeaveRequestDto>>
        GetLeaveRequest(int id)
    {
        var leaveRequest = await _leaveRequestService
            .GetLeaveRequestAsync(id);

        if (leaveRequest == null)
        {
            return NotFound();
        }

        return Ok(leaveRequest);
    }

    // GET: api/employees/1/leaves
    [HttpGet("/api/employees/{employeeId}/leaves")]
    public async Task<ActionResult<IEnumerable<LeaveRequestDto>>>
        GetEmployeeLeaves(int employeeId)
    {
        var leaveRequests = await _leaveRequestService
            .GetEmployeeLeavesAsync(employeeId);

        if (leaveRequests == null)
        {
            return NotFound("Employee not found.");
        }

        return Ok(leaveRequests);
    }

    // PUT: api/leaves/1/approve
    [HttpPut("{id}/approve")]
    public async Task<IActionResult> ApproveLeaveRequest(
        int id,
        LeaveActionDto leaveActionDto)
    {
        var result = await _leaveRequestService
            .ApproveLeaveRequestAsync(id, leaveActionDto);

        if (!result)
        {
            return BadRequest(
                "Unable to approve leave request.");
        }

        return NoContent();
    }

    // PUT: api/leaves/1/reject
    [HttpPut("{id}/reject")]
    public async Task<IActionResult> RejectLeaveRequest(
        int id,
        LeaveActionDto leaveActionDto)
    {
        var result = await _leaveRequestService
            .RejectLeaveRequestAsync(id, leaveActionDto);

        if (!result)
        {
            return BadRequest(
                "Unable to reject leave request.");
        }

        return NoContent();
    }
}