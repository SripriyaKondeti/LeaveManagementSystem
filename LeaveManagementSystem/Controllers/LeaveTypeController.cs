using Microsoft.AspNetCore.Mvc;
using LeaveManagementSystem.Data;
using LeaveManagementSystem.DTOs;
using LeaveManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagementSystem.Controllers;

[Route("api/leavetypes")]
[ApiController]
public class LeaveTypeController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public LeaveTypeController(ApplicationDbContext context)
    {
        _context = context;
    }


    // GET: api/leavetypes
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LeaveTypeDto>>> GetLeaveTypes()
    {
        var leaveTypes = await _context.LeaveTypes
            .Select(l => new LeaveTypeDto
            {
                LeaveTypeId = l.LeaveTypeId,
                LeaveTypeName = l.LeaveTypeName,
                MaximumDays = l.MaximumDays,
                IsActive = l.IsActive
            })
            .ToListAsync();

        return Ok(leaveTypes);
    }


    // GET: api/leavetypes/1
    [HttpGet("{id}")]
    public async Task<ActionResult<LeaveTypeDto>> GetLeaveType(int id)
    {
        var leaveType = await _context.LeaveTypes
            .Where(l => l.LeaveTypeId == id)
            .Select(l => new LeaveTypeDto
            {
                LeaveTypeId = l.LeaveTypeId,
                LeaveTypeName = l.LeaveTypeName,
                MaximumDays = l.MaximumDays,
                IsActive = l.IsActive
            })
            .FirstOrDefaultAsync();

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
        var leaveType = new LeaveType
        {
            LeaveTypeName = leaveTypeDto.LeaveTypeName,
            MaximumDays = leaveTypeDto.MaximumDays,
            IsActive = leaveTypeDto.IsActive
        };

        _context.LeaveTypes.Add(leaveType);

        await _context.SaveChangesAsync();

        leaveTypeDto.LeaveTypeId = leaveType.LeaveTypeId;

        return CreatedAtAction(
            nameof(GetLeaveType),
            new { id = leaveType.LeaveTypeId },
            leaveTypeDto
        );
    }


    // PUT: api/leavetypes/1
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateLeaveType(
        int id,
        LeaveTypeDto leaveTypeDto)
    {
        var leaveType = await _context.LeaveTypes.FindAsync(id);

        if (leaveType == null)
        {
            return NotFound();
        }

        leaveType.LeaveTypeName = leaveTypeDto.LeaveTypeName;
        leaveType.MaximumDays = leaveTypeDto.MaximumDays;
        leaveType.IsActive = leaveTypeDto.IsActive;

        await _context.SaveChangesAsync();

        return NoContent();
    }


    // DELETE: api/leavetypes/1
    // Soft Delete / Deactivate
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeactivateLeaveType(int id)
    {
        var leaveType = await _context.LeaveTypes.FindAsync(id);

        if (leaveType == null)
        {
            return NotFound();
        }

        leaveType.IsActive = false;

        await _context.SaveChangesAsync();

        return NoContent();
    }
}