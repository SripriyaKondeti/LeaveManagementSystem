using Microsoft.AspNetCore.Mvc;
using LeaveManagementSystem.DTOs;
using LeaveManagementSystem.Services;

namespace LeaveManagementSystem.Controllers;

[Route("api/employees")]
[ApiController]
public class EmployeeController : ControllerBase
{
    private readonly EmployeeService _employeeService;

    public EmployeeController(EmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    // GET: api/employees
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetEmployees()
    {
        var employees = await _employeeService.GetEmployeesAsync();

        return Ok(employees);
    }

    // GET: api/employees/1
    [HttpGet("{id}")]
    public async Task<ActionResult<EmployeeDto>> GetEmployee(int id)
    {
        var employee = await _employeeService.GetEmployeeAsync(id);

        if (employee == null)
        {
            return NotFound();
        }

        return Ok(employee);
    }

    // POST: api/employees
    [HttpPost]
    public async Task<ActionResult<EmployeeDto>> CreateEmployee(
        EmployeeDto employeeDto)
    {
        var employee =
            await _employeeService.CreateEmployeeAsync(employeeDto);

        return CreatedAtAction(
            nameof(GetEmployee),
            new { id = employee.EmployeeId },
            employee);
    }

    // PUT: api/employees/1
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEmployee(
        int id,
        EmployeeDto employeeDto)
    {
        var updated =
            await _employeeService.UpdateEmployeeAsync(id, employeeDto);

        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    // DELETE: api/employees/1
    // Soft Delete / Deactivate
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeactivateEmployee(int id)
    {
        var deactivated =
            await _employeeService.DeactivateEmployeeAsync(id);

        if (!deactivated)
        {
            return NotFound();
        }

        return NoContent();
    }
}