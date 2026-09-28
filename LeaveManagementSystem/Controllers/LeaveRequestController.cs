using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LeaveManagementSystem.DTOs;
using LeaveManagementSystem.Services;
using System.Security.Claims;

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
    public async Task<ActionResult<ApiResponseDto<LeaveRequestDto>>> CreateLeaveRequest(
        CreateLeaveRequestDto leaveRequestDto)
    {
        if (!User.IsInRole("Manager"))
        {
            if (!TryGetLoggedInEmployeeId(out var loggedInEmployeeId))
            {
                return StatusCode(
                    StatusCodes.Status401Unauthorized,
                    ApiResponseDto<object>.Failure(401, "Employee information is missing from the token."));
            }

            if (loggedInEmployeeId != leaveRequestDto.EmployeeId)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponseDto<object>.Failure(403, "You can create a leave request only for yourself."));
            }
        }

        var result = await _leaveRequestService
            .CreateLeaveRequestAsync(leaveRequestDto);

        return CreatedAtAction(
            nameof(GetLeaveRequest),
            new { id = result.LeaveRequestId },
            ApiResponseDto<LeaveRequestDto>.Success(201, "Leave request created successfully.", result));
    }


    // GET: api/leaves
    // Manager can view all.
    // Employee can view only their own requests.
    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<LeaveRequestDto>>>>
        GetLeaveRequests(
            int? employeeId,
            int? leaveTypeId,
            string? status,
            DateTime? fromDate,
            DateTime? toDate)
    {
        if (!User.IsInRole("Manager"))
        {
            if (!TryGetLoggedInEmployeeId(out var loggedInEmployeeId))
            {
                return StatusCode(
                    StatusCodes.Status401Unauthorized,
                    ApiResponseDto<object>.Failure(401, "Employee information is missing from the token."));
            }

            // Employee cannot request another employee's data.
            if (employeeId.HasValue &&
                employeeId.Value != loggedInEmployeeId)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponseDto<object>.Failure(403, "You don't have access to these leave requests."));
            }

            // Force employee filter to logged-in employee.
            employeeId = loggedInEmployeeId;
        }

        var leaveRequests = await _leaveRequestService
            .GetLeaveRequestsAsync(
                employeeId,
                leaveTypeId,
                status,
                fromDate,
                toDate);

        return Ok(ApiResponseDto<IEnumerable<LeaveRequestDto>>.Success(200, "Leave requests retrieved successfully.", leaveRequests));
    }


    // GET: api/leaves/{id}
    // Manager can view any request.
    // Employee can view only their own request.
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponseDto<LeaveRequestDto>>>
        GetLeaveRequest(int id)
    {
        if (id <= 0)
        {
            return BadRequest(ApiResponseDto<object>.Failure(400, "Leave request ID must be greater than zero."));
        }

        var loggedInEmployeeId = 0;

        if (!User.IsInRole("Manager") && !TryGetLoggedInEmployeeId(out loggedInEmployeeId))
        {
            return StatusCode(StatusCodes.Status401Unauthorized,
                ApiResponseDto<object>.Failure(401, "Employee information is missing from the token."));
        }

        var leaveRequest = await _leaveRequestService
            .GetLeaveRequestAsync(id);

        if (leaveRequest == null)
        {
            return NotFound(ApiResponseDto<object>.Failure(404, $"Leave request with ID {id} does not exist."));
        }

        if (!User.IsInRole("Manager"))
        {
            if (leaveRequest.EmployeeId != loggedInEmployeeId)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponseDto<object>.Failure(403, "You don't have access to this leave request."));
            }
        }

        return Ok(ApiResponseDto<LeaveRequestDto>.Success(200, "Leave request retrieved successfully.", leaveRequest));
    }


    // GET: /api/employees/{employeeId}/leaves
    // Manager can view any employee's history.
    // Employee can view only their own history.
    [HttpGet("/api/employees/{employeeId}/leaves")]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<LeaveRequestDto>>>>
        GetEmployeeLeaves(int employeeId)
    {
        if (employeeId <= 0)
        {
            return BadRequest(ApiResponseDto<object>.Failure(400, "Employee ID must be greater than zero."));
        }

        var loggedInEmployeeId = 0;

        if (!User.IsInRole("Manager") && !TryGetLoggedInEmployeeId(out loggedInEmployeeId))
        {
            return StatusCode(StatusCodes.Status401Unauthorized,
                ApiResponseDto<object>.Failure(401, "Employee information is missing from the token."));
        }

        if (!User.IsInRole("Manager") && loggedInEmployeeId != employeeId)
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                ApiResponseDto<object>.Failure(403, "You don't have access to this employee's leave history."));
        }

        var leaveRequests = await _leaveRequestService
            .GetEmployeeLeavesAsync(employeeId);

        if (leaveRequests == null)
        {
            return NotFound(ApiResponseDto<object>.Failure(404, $"Employee with ID {employeeId} does not exist."));
        }

        return Ok(ApiResponseDto<IEnumerable<LeaveRequestDto>>.Success(200, "Employee leave history retrieved successfully.", leaveRequests));
    }


    // PUT: api/leaves/{id}/approve
    // Manager only.
    [HttpPut("{id}/approve")]
    public async Task<ActionResult<ApiResponseDto<LeaveRequestDto>>> ApproveLeaveRequest(
        int id,
        LeaveActionDto leaveActionDto)
    {
        if (id <= 0)
        {
            return BadRequest(ApiResponseDto<object>.Failure(400, "Leave request ID must be greater than zero."));
        }

        if (leaveActionDto == null)
        {
            return BadRequest(ApiResponseDto<object>.Failure(400, "Approval payload is required."));
        }

        if (!User.IsInRole("Manager"))
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                ApiResponseDto<object>.Failure(403, "Only managers can approve leave requests."));
        }

        leaveActionDto.ApprovedBy = User.FindFirstValue(ClaimTypes.Name);

        var result = await _leaveRequestService
            .ApproveLeaveRequestAsync(id, leaveActionDto);

        return Ok(ApiResponseDto<LeaveRequestDto>.Success(200, "Leave request approved successfully.", result));
    }


    // PUT: api/leaves/{id}/reject
    // Manager only.
    [HttpPut("{id}/reject")]
    public async Task<ActionResult<ApiResponseDto<LeaveRequestDto>>> RejectLeaveRequest(
        int id,
        LeaveActionDto leaveActionDto)
    {
        if (id <= 0)
        {
            return BadRequest(ApiResponseDto<object>.Failure(400, "Leave request ID must be greater than zero."));
        }

        if (leaveActionDto == null)
        {
            return BadRequest(ApiResponseDto<object>.Failure(400, "Rejection payload is required."));
        }

        if (!User.IsInRole("Manager"))
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                ApiResponseDto<object>.Failure(403, "Only managers can reject leave requests."));
        }

        leaveActionDto.ApprovedBy = User.FindFirstValue(ClaimTypes.Name);

        var result = await _leaveRequestService
            .RejectLeaveRequestAsync(id, leaveActionDto);

        return Ok(ApiResponseDto<LeaveRequestDto>.Success(200, "Leave request rejected successfully.", result));
    }


    // Helper method
    // Gets EmployeeId from the JWT token.
    private bool TryGetLoggedInEmployeeId(
        out int employeeId)
    {
        employeeId = 0;

        var employeeIdClaim =
            User.FindFirst("EmployeeId")?.Value;

        return int.TryParse(
            employeeIdClaim,
            out employeeId);
    }
}
