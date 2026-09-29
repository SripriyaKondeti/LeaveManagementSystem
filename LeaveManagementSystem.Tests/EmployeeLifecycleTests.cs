using LeaveManagementSystem.Data;
using LeaveManagementSystem.DTOs;
using LeaveManagementSystem.Models;
using LeaveManagementSystem.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LeaveManagementSystem.Tests;

public class EmployeeLifecycleTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);

        context.Employees.Add(new Employee
        {
            EmployeeId = 1,
            EmployeeCode = "EMP001",
            Name = "Test Employee",
            Email = "test@example.com",
            Department = "IT",
            JoiningDate = DateTime.Today.AddDays(-365),
            IsActive = true
        });

        context.LeaveTypes.Add(new LeaveType
        {
            LeaveTypeId = 1,
            LeaveTypeName = "Annual Leave",
            MaximumDays = 15,
            IsActive = true
        });

        context.LeaveTypes.Add(new LeaveType
        {
            LeaveTypeId = 2,
            LeaveTypeName = "Sick Leave",
            MaximumDays = 10,
            IsActive = false
        });

        context.LeaveBalances.AddRange(
            new LeaveBalance { LeaveBalanceId = 1, EmployeeId = 1, LeaveTypeId = 1, TotalDays = 15, UsedDays = 3 },
            new LeaveBalance { LeaveBalanceId = 2, EmployeeId = 1, LeaveTypeId = 2, TotalDays = 10, UsedDays = 2 }
        );

        context.SaveChanges();
        return context;
    }

    [Fact]
    public async Task GetLeaveBalance_ExcludesInactiveLeaveTypeImmediately()
    {
        await using var context = CreateContext();
        var leaveTypeService = new LeaveTypeService(context);
        var balanceService = new LeaveBalanceService(context);

        var deactivated = await leaveTypeService.DeactivateLeaveTypeAsync(2);
        Assert.True(deactivated);

        var balances = (await balanceService.GetLeaveBalanceAsync(1))!.ToList();

        Assert.Single(balances);
        Assert.Equal("Annual Leave", balances[0].LeaveType);
        Assert.Equal(15, balances[0].TotalDays);
    }

    [Fact]
    public async Task DeactivateThenReactivateEmployee_RestoresOldBalances()
    {
        await using var context = CreateContext();
        var service = new EmployeeService(context);
        var balanceService = new LeaveBalanceService(context);

        Assert.True(await service.DeactivateEmployeeAsync(1));

        await service.UpdateEmployeeAsync(1, new EmployeeDto
        {
            EmployeeId = 1,
            EmployeeCode = "EMP001",
            Name = "Test Employee",
            Email = "test@example.com",
            Department = "IT",
            JoiningDate = DateTime.Today.AddDays(-365),
            IsActive = true
        }, true);

        var balances = (await balanceService.GetLeaveBalanceAsync(1))!.ToList();

        var annual = Assert.Single(balances);
        Assert.Equal(15, annual.TotalDays);
        Assert.Equal(3, annual.UsedDays);
        Assert.Equal(12, annual.RemainingDays);
    }

    [Fact]
    public async Task UpdateEmployee_WhenNotManager_DoesNotChangeActiveStatus()
    {
        await using var context = CreateContext();
        var service = new EmployeeService(context);

        var employee = await context.Employees.SingleAsync(e => e.EmployeeId == 1);
        var originalStatus = employee.IsActive;

        var updated = await service.UpdateEmployeeAsync(1, new EmployeeDto
        {
            EmployeeId = 1,
            EmployeeCode = "EMP001",
            Name = "Test Employee",
            Email = "test@example.com",
            Department = "IT",
            JoiningDate = DateTime.Today.AddDays(-365),
            IsActive = !originalStatus
        }, false);

        Assert.NotNull(updated);
        Assert.Equal(originalStatus, updated!.IsActive);
        Assert.Equal(originalStatus, (await context.Employees.FindAsync(1))!.IsActive);
    }
}
