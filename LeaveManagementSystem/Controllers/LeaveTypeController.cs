using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LeaveManagementSystem.DTOs;
using LeaveManagementSystem.Services;

namespace LeaveManagementSystem.Controllers;

[Route("api/leavetypes")]
[ApiController]
[Authorize]
public class LeaveTypeController : ControllerBase
{
    private readonly LeaveTypeService _leaveTypeService;

    public LeaveTypeController(LeaveTypeService leaveTypeService)
    {
        _leaveTypeService = leaveTypeService;
    }

    // GET: api/leavetypes
    // Managers and employees
    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<IEnumerable<LeaveTypeDto>>>> GetLeaveTypes()
    {
        var leaveTypes = await _leaveTypeService
            .GetLeaveTypesAsync();

        return Ok(ApiResponseDto<IEnumerable<LeaveTypeDto>>.Success(200, "Leave types retrieved successfully.", leaveTypes));
    }

    // GET: api/leavetypes/1
    // Manager only
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponseDto<LeaveTypeDto>>> GetLeaveType(int id)
    {
        if (id <= 0)
        {
            return BadRequest(ApiResponseDto<object>.Failure(400, "Leave type ID must be greater than zero."));
        }

        if (!User.IsInRole("Manager"))
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                ApiResponseDto<object>.Failure(403, "Only managers can view leave types."));
        }

        var leaveType = await _leaveTypeService
            .GetLeaveTypeAsync(id);

        if (leaveType == null)
        {
            return NotFound(ApiResponseDto<object>.Failure(404, $"Leave type with ID {id} does not exist."));
        }

        return Ok(ApiResponseDto<LeaveTypeDto>.Success(200, "Leave type retrieved successfully.", leaveType));
    }

    // POST: api/leavetypes
    // Manager only
    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<LeaveTypeDto>>> CreateLeaveType(
        LeaveTypeDto leaveTypeDto)
    {
        if (!User.IsInRole("Manager"))
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                ApiResponseDto<object>.Failure(403, "Only managers can create leave types."));
        }

        var leaveType = await _leaveTypeService
            .CreateLeaveTypeAsync(leaveTypeDto);

        return CreatedAtAction(
            nameof(GetLeaveType),
            new { id = leaveType.LeaveTypeId },
            ApiResponseDto<LeaveTypeDto>.Success(201, "Leave type created successfully.", leaveType));
    }

    // PUT: api/leavetypes/1
    // Manager only
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateLeaveType(
        int id,
        LeaveTypeDto leaveTypeDto)
    {
        if (id <= 0)
        {
            return BadRequest(ApiResponseDto<object>.Failure(400, "Leave type ID must be greater than zero."));
        }

        if (!User.IsInRole("Manager"))
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                ApiResponseDto<object>.Failure(403, "Only managers can update leave types."));
        }

        var updatedLeaveType = await _leaveTypeService
            .UpdateLeaveTypeAsync(id, leaveTypeDto);

        if (updatedLeaveType == null)
        {
            return NotFound(ApiResponseDto<object>.Failure(404, $"Leave type with ID {id} does not exist."));
        }

        return Ok(ApiResponseDto<LeaveTypeDto>.Success(200, "Leave type updated successfully.", updatedLeaveType));
    }

    // DELETE: api/leavetypes/1
    // Soft Delete / Deactivate
    // Manager only
    [HttpDelete("{id}")]
    public async Task<ActionResult<object>> DeactivateLeaveType(int id)
    {
        if (id <= 0)
        {
            return BadRequest(ApiResponseDto<object>.Failure(400, "Leave type ID must be greater than zero."));
        }

        if (!User.IsInRole("Manager"))
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                ApiResponseDto<object>.Failure(403, "Only managers can deactivate leave types."));
        }

        var deactivated = await _leaveTypeService
            .DeactivateLeaveTypeAsync(id);

        if (!deactivated)
        {
            return NotFound(ApiResponseDto<object>.Failure(404, $"Leave type with ID {id} does not exist."));
        }

        return Ok(ApiResponseDto<object>.Success(200, $"Leave type with ID {id} was deactivated successfully.", null));
    }
}
