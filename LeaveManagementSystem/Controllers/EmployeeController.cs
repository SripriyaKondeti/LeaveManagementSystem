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
    public async Task<ActionResult<ApiResponseDto<IEnumerable<EmployeeDto>>>> GetEmployees()
    {
        if (!User.IsInRole("Manager"))
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                ApiResponseDto<object>.Failure(StatusCodes.Status403Forbidden, "Only managers can view all employees."));
        }

        var employees = await _employeeService.GetEmployeesAsync();

        return Ok(ApiResponseDto<IEnumerable<EmployeeDto>>.Success(StatusCodes.Status200OK, "Employees retrieved successfully.", employees));
    }

    // GET: api/employees/1
    // Employee can view only themselves.
    // Manager can view anyone.
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponseDto<EmployeeDto>>> GetEmployee(int id)
    {
        if (id <= 0)
        {
            return BadRequest(ApiResponseDto<object>.Failure(400, "Employee ID must be greater than zero."));
        }

        if (!User.IsInRole("Manager"))
        {
            var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;

            if (!int.TryParse(employeeIdClaim, out var loggedInEmployeeId))
            {
                return StatusCode(StatusCodes.Status401Unauthorized,
                    ApiResponseDto<object>.Failure(401, "Employee information is missing from the token."));
            }

            if (loggedInEmployeeId != id)
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    ApiResponseDto<object>.Failure(403, "You don't have access to this employee."));
            }
        }

        var employee = await _employeeService.GetEmployeeAsync(id);

        if (employee == null)
        {
            return NotFound(ApiResponseDto<object>.Failure(404, $"Employee with ID {id} does not exist."));
        }

        return Ok(ApiResponseDto<EmployeeDto>.Success(200, "Employee retrieved successfully.", employee));
    }

    // POST: api/employees
    // Manager only
    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<EmployeeDto>>> CreateEmployee(
        CreateEmployeeDto employeeDto)
    {
        if (!User.IsInRole("Manager"))
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                ApiResponseDto<object>.Failure(403, "Only managers can create employees."));
        }

        var employee = await _employeeService
            .CreateEmployeeAsync(employeeDto);

        return CreatedAtAction(
            nameof(GetEmployee),
            new { id = employee.EmployeeId },
            ApiResponseDto<EmployeeDto>.Success(201, "Employee created successfully.", employee));
    }

    // PUT: api/employees/{id}
    // Manager can update any employee.
    // Employee can update only themselves.
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEmployee(
        int id,
        EmployeeDto employeeDto)
    {
        if (id <= 0)
        {
            return BadRequest(ApiResponseDto<object>.Failure(400, "Employee ID must be greater than zero."));
        }

        var isManager = User.IsInRole("Manager");

        if (!isManager)
        {
            var employeeIdClaim =
                User.FindFirst("EmployeeId")?.Value;

            if (!int.TryParse(
                    employeeIdClaim,
                    out var loggedInEmployeeId))
            {
                return StatusCode(
                    StatusCodes.Status401Unauthorized,
                    ApiResponseDto<object>.Failure(401, "Employee information is missing from the token."));
            }

            if (loggedInEmployeeId != id)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponseDto<object>.Failure(403, "You can update only your own employee information."));
            }

            var existingEmployee = await _employeeService.GetEmployeeAsync(id);
            if (existingEmployee == null)
            {
                return NotFound(ApiResponseDto<object>.Failure(404, $"Employee with ID {id} does not exist."));
            }

            if (employeeDto.IsActive != existingEmployee.IsActive)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponseDto<object>.Failure(403, "Employees cannot change the active status. Only managers can do this."));
            }

            employeeDto.IsActive = existingEmployee.IsActive;
        }

        var updatedEmployee = await _employeeService
            .UpdateEmployeeAsync(
                id,
                employeeDto,
                isManager);

        if (updatedEmployee == null)
        {
            return NotFound(ApiResponseDto<object>.Failure(404, $"Employee with ID {id} does not exist."));
        }

        return Ok(ApiResponseDto<EmployeeDto>.Success(200, "Employee updated successfully.", updatedEmployee));
    }
    // DELETE: api/employees/1
    // Soft Delete / Deactivate
    // Manager only
    [HttpDelete("{id}")]
    public async Task<ActionResult<object>> DeactivateEmployee(int id)
    {
        if (!User.IsInRole("Manager"))
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                ApiResponseDto<object>.Failure(403, "Only managers can deactivate employees."));
        }

        var deactivated = await _employeeService
            .DeactivateEmployeeAsync(id);

        if (!deactivated)
        {
            return NotFound(ApiResponseDto<object>.Failure(404, $"Employee with ID {id} does not exist."));
        }

        return Ok(ApiResponseDto<object>.Success(200, $"Employee with ID {id} was deactivated successfully.", null));
    }
}
