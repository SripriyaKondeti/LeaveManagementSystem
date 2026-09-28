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
    // Manager only
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LeaveTypeDto>>> GetLeaveTypes()
    {
        if (!User.IsInRole("Manager"))
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                "Only managers can view leave types.");
        }

        var leaveTypes = await _leaveTypeService
            .GetLeaveTypesAsync();

        return Ok(leaveTypes);
    }

    // GET: api/leavetypes/1
    // Manager only
    [HttpGet("{id}")]
    public async Task<ActionResult<LeaveTypeDto>> GetLeaveType(int id)
    {
        if (!User.IsInRole("Manager"))
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                "Only managers can view leave types.");
        }

        var leaveType = await _leaveTypeService
            .GetLeaveTypeAsync(id);

        if (leaveType == null)
        {
            return NotFound(
                $"Leave type with ID {id} does not exist.");
        }

        return Ok(leaveType);
    }

    // POST: api/leavetypes
    // Manager only
    [HttpPost]
    public async Task<ActionResult<LeaveTypeDto>> CreateLeaveType(
        LeaveTypeDto leaveTypeDto)
    {
        if (!User.IsInRole("Manager"))
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                "Only managers can create leave types.");
        }

        var leaveType = await _leaveTypeService
            .CreateLeaveTypeAsync(leaveTypeDto);

        return CreatedAtAction(
            nameof(GetLeaveType),
            new { id = leaveType.LeaveTypeId },
            leaveType);
    }

    // PUT: api/leavetypes/1
    // Manager only
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateLeaveType(
        int id,
        LeaveTypeDto leaveTypeDto)
    {
        if (!User.IsInRole("Manager"))
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                "Only managers can update leave types.");
        }

        var updated = await _leaveTypeService
            .UpdateLeaveTypeAsync(id, leaveTypeDto);

        if (!updated)
        {
            return NotFound(
                $"Leave type with ID {id} does not exist.");
        }

        return NoContent();
    }

    // DELETE: api/leavetypes/1
    // Soft Delete / Deactivate
    // Manager only
    [HttpDelete("{id}")]
    public async Task<ActionResult<object>> DeactivateLeaveType(int id)
    {
        if (!User.IsInRole("Manager"))
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                "Only managers can deactivate leave types.");
        }

        var deactivated = await _leaveTypeService
            .DeactivateLeaveTypeAsync(id);

        if (!deactivated)
        {
            return NotFound(new
            {
                statusCode = StatusCodes.Status404NotFound,
                message = $"Leave type with ID {id} does not exist."
            });
        }

        return Ok(new
        {
            statusCode = StatusCodes.Status200OK,
            message = $"Leave type with ID {id} was deactivated successfully."
        });
    }
}