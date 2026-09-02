using Microsoft.AspNetCore.Mvc;
using LeaveManagementSystem.Data;
using LeaveManagementSystem.DTOs;
using LeaveManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagementSystem.Controllers;

[Route("api/employees")]
[ApiController]
public class EmployeeController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public EmployeeController(ApplicationDbContext context)
    {
        _context = context;
    }


    // GET: api/employees
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetEmployees()
    {
        var employees = await _context.Employees
            .Select(e => new EmployeeDto
            {
                EmployeeId = e.EmployeeId,
                EmployeeCode = e.EmployeeCode,
                Name = e.Name,
                Email = e.Email,
                Department = e.Department,
                JoiningDate = e.JoiningDate,
                IsActive = e.IsActive
            })
            .ToListAsync();

        return Ok(employees);
    }


    // GET: api/employees/1
    [HttpGet("{id}")]
    public async Task<ActionResult<EmployeeDto>> GetEmployee(int id)
    {
        var employee = await _context.Employees
            .Where(e => e.EmployeeId == id)
            .Select(e => new EmployeeDto
            {
                EmployeeId = e.EmployeeId,
                EmployeeCode = e.EmployeeCode,
                Name = e.Name,
                Email = e.Email,
                Department = e.Department,
                JoiningDate = e.JoiningDate,
                IsActive = e.IsActive
            })
            .FirstOrDefaultAsync();

        if (employee == null)
        {
            return NotFound();
        }

        return Ok(employee);
    }


    // POST: api/employees
    [HttpPost]
    public async Task<ActionResult<EmployeeDto>> CreateEmployee(EmployeeDto employeeDto)
    {
        var employee = new Employee
        {
            EmployeeCode = employeeDto.EmployeeCode,
            Name = employeeDto.Name,
            Email = employeeDto.Email,
            Department = employeeDto.Department,
            JoiningDate = employeeDto.JoiningDate,
            IsActive = employeeDto.IsActive
        };

        _context.Employees.Add(employee);

        await _context.SaveChangesAsync();

        employeeDto.EmployeeId = employee.EmployeeId;

        return CreatedAtAction(
            nameof(GetEmployee),
            new { id = employee.EmployeeId },
            employeeDto
        );
    }


    // PUT: api/employees/1
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEmployee(
    int id,
    EmployeeDto employeeDto)
    {
        var employee = await _context.Employees.FindAsync(id);

        if (employee == null)
        {
            return NotFound();
        }

        employee.EmployeeCode = employeeDto.EmployeeCode;
        employee.Name = employeeDto.Name;
        employee.Email = employeeDto.Email;
        employee.Department = employeeDto.Department;
        employee.JoiningDate = employeeDto.JoiningDate;
        employee.IsActive = employeeDto.IsActive;

        await _context.SaveChangesAsync();

        return NoContent();
    }


    // DELETE: api/employees/1
    // Soft Delete / Deactivate
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeactivateEmployee(int id)
    {
        var employee = await _context.Employees.FindAsync(id);

        if (employee == null)
        {
            return NotFound();
        }

        employee.IsActive = false;

        await _context.SaveChangesAsync();

        return NoContent();
    }
}