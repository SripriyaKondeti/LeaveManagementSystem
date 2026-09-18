using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LeaveManagementSystem.DTOs;
using LeaveManagementSystem.Services;

namespace LeaveManagementSystem.Controllers;

[Route("api/leaves")]
[ApiController]
[Authorize]
public class LeaveRequestController : ControllerBase
{
    private readonly LeaveRequestService _leaveRequestService;

    public LeaveRequestController(
        LeaveRequestService leaveRequestService)
    {
        _leaveRequestService = leaveRequestService;
    }

    // POST: api/leaves
    // Employee can create only for themselves.
    // Manager can create for any employee.
    [HttpPost]
    public async Task<ActionResult<LeaveRequestDto>> CreateLeaveRequest(
        CreateLeaveRequestDto leaveRequestDto)
    {
        if (!User.IsInRole("Manager"))
        {
            var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;

            if (!int.TryParse(
                    employeeIdClaim,
                    out var loggedInEmployeeId))
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    "Employee information is missing from the token.");
            }

            if (loggedInEmployeeId != leaveRequestDto.EmployeeId)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    "You can create a leave request only for yourself.");
            }
        }

        var result = await _leaveRequestService
            .CreateLeaveRequestAsync(leaveRequestDto);

        return CreatedAtAction(
            nameof(GetLeaveRequest),
            new { id = result.LeaveRequestId },
            result);
    }


    // GET: api/leaves
    // Manager can view all.
    // Employee can view only their own requests.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LeaveRequestDto>>>
        GetLeaveRequests(
            int? employeeId,
            int? leaveTypeId,
            string? status,
            DateTime? fromDate,
            DateTime? toDate)
    {
        if (!User.IsInRole("Manager"))
        {
            var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;

            if (!int.TryParse(
                    employeeIdClaim,
                    out var loggedInEmployeeId))
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    "Employee information is missing from the token.");
            }

            // Employee cannot request another employee's data
            if (employeeId.HasValue &&
                employeeId.Value != loggedInEmployeeId)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    "You don't have access to these leave requests.");
            }

            // Force the filter to the logged-in employee
            employeeId = loggedInEmployeeId;
        }

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
    // Manager can view any request.
    // Employee can view only their own request.
    [HttpGet("{id}")]
    public async Task<ActionResult<LeaveRequestDto>>
        GetLeaveRequest(int id)
    {
        var leaveRequest = await _leaveRequestService
            .GetLeaveRequestAsync(id);

        if (leaveRequest == null)
        {
            return NotFound(
                $"Leave request with ID {id} does not exist.");
        }

        if (!User.IsInRole("Manager"))
        {
            var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;

            if (!int.TryParse(
                    employeeIdClaim,
                    out var loggedInEmployeeId))
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    "Employee information is missing from the token.");
            }

            if (leaveRequest.EmployeeId != loggedInEmployeeId)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    "You don't have access to this leave request.");
            }
        }

        return Ok(leaveRequest);
    }


    // GET: api/employees/1/leaves
    // Manager can view any employee's history.
    // Employee can view only their own history.
    [HttpGet("/api/employees/{employeeId}/leaves")]
    public async Task<ActionResult<IEnumerable<LeaveRequestDto>>>
        GetEmployeeLeaves(int employeeId)
    {
        var leaveRequests = await _leaveRequestService
            .GetEmployeeLeavesAsync(employeeId);

        if (leaveRequests == null)
        {
            return NotFound(
                $"Employee with ID {employeeId} does not exist.");
        }

        if (!User.IsInRole("Manager"))
        {
            var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;

            if (!int.TryParse(
                    employeeIdClaim,
                    out var loggedInEmployeeId))
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    "Employee information is missing from the token.");
            }

            if (loggedInEmployeeId != employeeId)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    "You don't have access to this employee's leave history.");
            }
        }

        return Ok(leaveRequests);
    }


    // PUT: api/leaves/1/approve
    // Manager only
    [HttpPut("{id}/approve")]
    public async Task<IActionResult> ApproveLeaveRequest(
        int id,
        LeaveActionDto leaveActionDto)
    {
        if (!User.IsInRole("Manager"))
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                "Only managers can approve leave requests.");
        }

        await _leaveRequestService
            .ApproveLeaveRequestAsync(id, leaveActionDto);

        return NoContent();
    }


    // PUT: api/leaves/1/reject
    // Manager only
    [HttpPut("{id}/reject")]
    public async Task<IActionResult> RejectLeaveRequest(
        int id,
        LeaveActionDto leaveActionDto)
    {
        if (!User.IsInRole("Manager"))
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                "Only managers can reject leave requests.");
        }

        await _leaveRequestService
            .RejectLeaveRequestAsync(id, leaveActionDto);

        return NoContent();
    }
}