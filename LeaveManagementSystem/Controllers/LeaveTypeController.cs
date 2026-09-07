using Microsoft.AspNetCore.Mvc;
using LeaveManagementSystem.DTOs;
using LeaveManagementSystem.Services;

namespace LeaveManagementSystem.Controllers;

[Route("api/leavetypes")]
[ApiController]
public class LeaveTypeController : ControllerBase
{
    private readonly LeaveTypeService _leaveTypeService;

    public LeaveTypeController(LeaveTypeService leaveTypeService)
    {
        _leaveTypeService = leaveTypeService;
    }

    // GET: api/leavetypes
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LeaveTypeDto>>> GetLeaveTypes()
    {
        var leaveTypes = await _leaveTypeService
            .GetLeaveTypesAsync();

        return Ok(leaveTypes);
    }

    // GET: api/leavetypes/1
    [HttpGet("{id}")]
    public async Task<ActionResult<LeaveTypeDto>> GetLeaveType(int id)
    {
        var leaveType = await _leaveTypeService
            .GetLeaveTypeAsync(id);

        if (leaveType == null)
        {
            return NotFound();
        }

        return Ok(leaveType);
    }

    // POST: api/leavetypes
    [HttpPost]
    public async Task<ActionResult<LeaveTypeDto>> CreateLeaveType(
        LeaveTypeDto leaveTypeDto)
    {
        var leaveType = await _leaveTypeService
            .CreateLeaveTypeAsync(leaveTypeDto);

        return CreatedAtAction(
            nameof(GetLeaveType),
            new { id = leaveType.LeaveTypeId },
            leaveType
        );
    }

    // PUT: api/leavetypes/1
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateLeaveType(
        int id,
        LeaveTypeDto leaveTypeDto)
    {
        var updated = await _leaveTypeService
            .UpdateLeaveTypeAsync(id, leaveTypeDto);

        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    // DELETE: api/leavetypes/1
    // Soft Delete / Deactivate
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeactivateLeaveType(int id)
    {
        var deactivated = await _leaveTypeService
            .DeactivateLeaveTypeAsync(id);

        if (!deactivated)
        {
            return NotFound();
        }

        return NoContent();
    }
}