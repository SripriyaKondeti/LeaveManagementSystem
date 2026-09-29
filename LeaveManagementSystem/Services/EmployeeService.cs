using LeaveManagementSystem.Data;
using LeaveManagementSystem.DTOs;
using LeaveManagementSystem.Models;
using LeaveManagementSystem.Middleware;
using Microsoft.AspNetCore.Identity;
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
    CreateEmployeeDto employeeDto)
    {
        var employeeCode = employeeDto.EmployeeCode.Trim();
        var email = employeeDto.Email.Trim();
        var username = employeeDto.Username.Trim();

        if (await _context.Employees.AnyAsync(e =>
                e.EmployeeCode == employeeCode ||
                e.Email.ToLower() == email.ToLower()))
        {
            throw new ConflictException(
                "An employee with the same code or email already exists.");
        }

        if (await _context.Users.AnyAsync(u =>
                u.Username.ToLower() == username.ToLower()))
        {
            throw new ConflictException(
                $"Username '{username}' is already taken.");
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

        var user = new User
        {
            Username = username,
            Role = employeeDto.Role,
            Employee = employee
        };

        user.PasswordHash = new PasswordHasher<User>()
            .HashPassword(user, employeeDto.Password);

        _context.Users.Add(user);

        if (employee.IsActive)
        {
            var activeLeaveTypes = await _context.LeaveTypes
                .Where(l => l.IsActive)
                .ToListAsync();

            _context.LeaveBalances.AddRange(activeLeaveTypes
                .Select(leaveType => new LeaveBalance
                {
                    Employee = employee,
                    LeaveTypeId = leaveType.LeaveTypeId,
                    TotalDays = leaveType.MaximumDays,
                    UsedDays = 0
                }));
        }

        // Single save so the employee, login and balances are created together.
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

        var wasInactive = !employee.IsActive;

        if (canChangeActiveStatus)
        {
            employee.IsActive = employeeDto.IsActive;
        }
        else
        {
            employeeDto.IsActive = employee.IsActive;
        }

        if (wasInactive && employee.IsActive)
        {
            var activeLeaveTypes = await _context.LeaveTypes
                .Where(l => l.IsActive)
                .ToListAsync();

            var existingLeaveTypeIds = await _context.LeaveBalances
                .Where(b => b.EmployeeId == id)
                .Select(b => b.LeaveTypeId)
                .ToListAsync();

            // Old balances are kept; only leave types added while inactive get a new balance.
            var missingBalances = activeLeaveTypes
                .Where(l => !existingLeaveTypeIds.Contains(l.LeaveTypeId))
                .Select(l => new LeaveBalance
                {
                    EmployeeId = id,
                    LeaveTypeId = l.LeaveTypeId,
                    TotalDays = l.MaximumDays,
                    UsedDays = 0
                })
                .ToList();

            _context.LeaveBalances.AddRange(missingBalances);
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

        await _context.SaveChangesAsync();

        return true;
    }
}