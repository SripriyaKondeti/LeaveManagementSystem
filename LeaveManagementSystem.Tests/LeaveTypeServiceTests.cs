using LeaveManagementSystem.Data;
using LeaveManagementSystem.DTOs;
using LeaveManagementSystem.Models;
using LeaveManagementSystem.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LeaveManagementSystem.Tests;

public class LeaveTypeServiceTests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);

        context.Employees.AddRange(
            new Employee
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

        context.LeaveBalances.Add(new LeaveBalance
        {
            LeaveBalanceId = 1,
            EmployeeId = 1,
            LeaveTypeId = 1,
            TotalDays = 15,
            UsedDays = 3
        });

        context.SaveChanges();
        return context;
    }

    [Fact]
    public async Task DeactivateLeaveType_ZeroesBalanceForThatType()
    {
        await using var context = CreateContext();
        var service = new LeaveTypeService(context);

        var result = await service.DeactivateLeaveTypeAsync(1);

        Assert.True(result);

        var balance = await context.LeaveBalances
            .SingleAsync(b => b.LeaveTypeId == 1 && b.EmployeeId == 1);

        Assert.Equal(0, balance.TotalDays);
        Assert.Equal(0, balance.UsedDays);
    }

    [Fact]
    public async Task UpdateLeaveType_WhenSetInactive_ZeroesBalanceForThatType()
    {
        await using var context = CreateContext();
        var service = new LeaveTypeService(context);

        var updated = await service.UpdateLeaveTypeAsync(1, new LeaveTypeDto
        {
            LeaveTypeId = 1,
            LeaveTypeName = "Annual Leave",
            MaximumDays = 15,
            IsActive = false
        });

        Assert.NotNull(updated);
        Assert.False(updated!.IsActive);

        var balance = await context.LeaveBalances
            .SingleAsync(b => b.LeaveTypeId == 1 && b.EmployeeId == 1);

        Assert.Equal(0, balance.TotalDays);
        Assert.Equal(0, balance.UsedDays);
    }
}
