using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LeaveManagementSystem.DTOs;
using LeaveManagementSystem.Services;

namespace LeaveManagementSystem.Controllers;

[Route("api/employees")]
[ApiController]
[Authorize]
public class EmployeeController : ControllerBase
{
    private readonly EmployeeService _employeeService;

    public EmployeeController(EmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    // GET: api/employees
    // Manager only
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetEmployees()
    {
        if (!User.IsInRole("Manager"))
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                "Only managers can view all employees.");
        }

        var employees = await _employeeService.GetEmployeesAsync();

        return Ok(employees);
    }

    // GET: api/employees/1
    // Employee can view only themselves.
    // Manager can view anyone.
    [HttpGet("{id}")]
    public async Task<ActionResult<EmployeeDto>> GetEmployee(int id)
    {
        var employee = await _employeeService.GetEmployeeAsync(id);

        if (employee == null)
        {
            return NotFound(
                $"Employee with ID {id} does not exist.");
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

            if (loggedInEmployeeId != id)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    "You don't have access to this employee.");
            }
        }

        return Ok(employee);
    }

    // POST: api/employees
    // Manager only
    [HttpPost]
    public async Task<ActionResult<EmployeeDto>> CreateEmployee(
        EmployeeDto employeeDto)
    {
        if (!User.IsInRole("Manager"))
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                "Only managers can create employees.");
        }

        var employee = await _employeeService
            .CreateEmployeeAsync(employeeDto);

        return CreatedAtAction(
            nameof(GetEmployee),
            new { id = employee.EmployeeId },
            employee);
    }

    // PUT: api/employees/1
    // Manager only
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEmployee(
        int id,
        EmployeeDto employeeDto)
    {
        if (!User.IsInRole("Manager"))
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                "Only managers can update employees.");
        }

        var updated = await _employeeService
            .UpdateEmployeeAsync(id, employeeDto);

        if (!updated)
        {
            return NotFound(
                $"Employee with ID {id} does not exist.");
        }

        return NoContent();
    }

    // DELETE: api/employees/1
    // Soft Delete / Deactivate
    // Manager only
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeactivateEmployee(int id)
    {
        if (!User.IsInRole("Manager"))
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                "Only managers can deactivate employees.");
        }

        var deactivated = await _employeeService
            .DeactivateEmployeeAsync(id);

        if (!deactivated)
        {
            return NotFound(
                $"Employee with ID {id} does not exist.");
        }

        return NoContent();
    }
}