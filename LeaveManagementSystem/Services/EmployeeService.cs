using LeaveManagementSystem.Data;
using LeaveManagementSystem.DTOs;
using LeaveManagementSystem.Models;
using LeaveManagementSystem.Middleware;
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
        var employeeCode = employeeDto.EmployeeCode.Trim();
        var email = employeeDto.Email.Trim();

        if (await _context.Employees.AnyAsync(e =>
                e.EmployeeCode == employeeCode ||
                e.Email.ToLower() == email.ToLower()))
        {
            throw new ConflictException(
                "An employee with the same code or email already exists.");
        }

        var employee = new Employee
        {
            EmployeeCode = employeeCode,
            Name = employeeDto.Name,
            Email = email,
            Department = employeeDto.Department,
            JoiningDate = employeeDto.JoiningDate,
            IsActive = employeeDto.IsActive
        };

        _context.Employees.Add(employee);

        await _context.SaveChangesAsync();

        // Create leave balances for the new employee
        if (employee.IsActive)
        {
            var activeLeaveTypes = await _context.LeaveTypes
                .Where(l => l.IsActive)
                .ToListAsync();

            var leaveBalances = activeLeaveTypes
                .Select(leaveType => new LeaveBalance
                {
                    EmployeeId = employee.EmployeeId,
                    LeaveTypeId = leaveType.LeaveTypeId,
                    TotalDays = leaveType.MaximumDays,
                    UsedDays = 0
                })
                .ToList();

            _context.LeaveBalances.AddRange(leaveBalances);

            await _context.SaveChangesAsync();
        }

        employeeDto.EmployeeId = employee.EmployeeId;

        return employeeDto;
    }

    public async Task<EmployeeDto?> UpdateEmployeeAsync(
        int id,
        EmployeeDto employeeDto,
        bool canChangeActiveStatus)
    {
        var employee = await _context.Employees.FindAsync(id);

        if (employee == null)
        {
            return null;
        }

        var employeeCode = employeeDto.EmployeeCode.Trim();
        var email = employeeDto.Email.Trim();

        if (await _context.Employees.AnyAsync(e =>
                e.EmployeeId != id &&
                (e.EmployeeCode == employeeCode ||
                 e.Email.ToLower() == email.ToLower())))
        {
            throw new ConflictException(
                "An employee with the same code or email already exists.");
        }

        employee.EmployeeCode = employeeCode;
        employee.Name = employeeDto.Name;
        employee.Email = email;
        employee.Department = employeeDto.Department;
        employee.JoiningDate = employeeDto.JoiningDate;

        if (canChangeActiveStatus)
        {
            employee.IsActive = employeeDto.IsActive;
        }
        else
        {
            employeeDto.IsActive = employee.IsActive;
        }

        await _context.SaveChangesAsync();

        return new EmployeeDto
        {
            EmployeeId = employee.EmployeeId,
            EmployeeCode = employee.EmployeeCode,
            Name = employee.Name,
            Email = employee.Email,
            Department = employee.Department,
            JoiningDate = employee.JoiningDate,
            IsActive = employee.IsActive
        };
    }

    public async Task<bool> DeactivateEmployeeAsync(int id)
    {
        var employee = await _context.Employees.FindAsync(id);

        if (employee == null)
        {
            return false;
        }

        employee.IsActive = false;

        var employeeBalances = await _context.LeaveBalances
            .Where(b => b.EmployeeId == id)
            .ToListAsync();

        foreach (var balance in employeeBalances)
        {
            balance.TotalDays = 0;
            balance.UsedDays = 0;
        }

        await _context.SaveChangesAsync();

        return true;
    }
}