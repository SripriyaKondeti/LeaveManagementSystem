using LeaveManagementSystem.Data;
using LeaveManagementSystem.DTOs;
using LeaveManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagementSystem.Services;

public class EmployeeService
{
    private readonly ApplicationDbContext _context;

    public EmployeeService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<EmployeeDto>> GetEmployeesAsync()
    {
        return await _context.Employees
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
    }

    public async Task<EmployeeDto?> GetEmployeeAsync(int id)
    {
        return await _context.Employees
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
    }

    public async Task<EmployeeDto> CreateEmployeeAsync(
        EmployeeDto employeeDto)
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

        return employeeDto;
    }

    public async Task<bool> UpdateEmployeeAsync(
        int id,
        EmployeeDto employeeDto)
    {
        var employee = await _context.Employees.FindAsync(id);

        if (employee == null)
        {
            return false;
        }

        employee.EmployeeCode = employeeDto.EmployeeCode;
        employee.Name = employeeDto.Name;
        employee.Email = employeeDto.Email;
        employee.Department = employeeDto.Department;
        employee.JoiningDate = employeeDto.JoiningDate;
        employee.IsActive = employeeDto.IsActive;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeactivateEmployeeAsync(int id)
    {
        var employee = await _context.Employees.FindAsync(id);

        if (employee == null)
        {
            return false;
        }

        employee.IsActive = false;

        await _context.SaveChangesAsync();

        return true;
    }
}