using LeaveManagementSystem.Data;
using LeaveManagementSystem.DTOs;
using LeaveManagementSystem.Middleware;
using LeaveManagementSystem.Models;
using LeaveManagementSystem.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LeaveManagementSystem.Tests;

public class LeaveRequestServiceTests
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
            },
            new Employee
            {
                EmployeeId = 2,
                EmployeeCode = "EMP002",
                Name = "Other Employee",
                Email = "other@example.com",
                Department = "HR",
                JoiningDate = DateTime.Today.AddDays(-200),
                IsActive = true
            });

        context.LeaveTypes.AddRange(
            new LeaveType { LeaveTypeId = 1, LeaveTypeName = "Annual Leave", MaximumDays = 15, IsActive = true },
            new LeaveType { LeaveTypeId = 2, LeaveTypeName = "Sick Leave", MaximumDays = 10, IsActive = true });

        context.LeaveBalances.AddRange(
            new LeaveBalance { LeaveBalanceId = 1, EmployeeId = 1, LeaveTypeId = 1, TotalDays = 10, UsedDays = 0 },
            new LeaveBalance { LeaveBalanceId = 2, EmployeeId = 1, LeaveTypeId = 2, TotalDays = 10, UsedDays = 0 },
            new LeaveBalance { LeaveBalanceId = 3, EmployeeId = 2, LeaveTypeId = 1, TotalDays = 10, UsedDays = 0 });

        context.LeaveRequests.Add(new LeaveRequest
        {
            LeaveRequestId = 1,
            EmployeeId = 1,
            LeaveTypeId = 1,
            FromDate = DateTime.Today.AddDays(5),
            ToDate = DateTime.Today.AddDays(6),
            Reason = "Existing",
            Status = "Approved",
            AppliedDate = DateTime.Today,
            ApprovedBy = "Manager",
            Comments = "Approved"
        });

        context.SaveChanges();
        return context;
    }

    [Fact]
    public async Task CreateLeaveRequest_RejectsPastDateRequest()
    {
        await using var context = CreateContext();
        var service = new LeaveRequestService(context);

        var dto = new CreateLeaveRequestDto
        {
            EmployeeId = 1,
            LeaveTypeId = 1,
            FromDate = DateTime.Today.AddDays(-1),
            ToDate = DateTime.Today.AddDays(1),
            Reason = "Vacation"
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateLeaveRequestAsync(dto));
        Assert.Contains("cannot start before today", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateLeaveRequest_RejectsOverlappingApprovedLeave()
    {
        await using var context = CreateContext();
        var service = new LeaveRequestService(context);

        var dto = new CreateLeaveRequestDto
        {
            EmployeeId = 1,
            LeaveTypeId = 1,
            FromDate = DateTime.Today.AddDays(6),
            ToDate = DateTime.Today.AddDays(8),
            Reason = "Overlap"
        };

        var ex = await Assert.ThrowsAsync<ConflictException>(() => service.CreateLeaveRequestAsync(dto));
        Assert.Contains("overlap", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ApproveLeaveRequest_RejectsIfNotEnoughRemainingBalance()
    {
        await using var context = CreateContext();
        var service = new LeaveRequestService(context);

        var leaveRequest = new LeaveRequest
        {
            LeaveRequestId = 2,
            EmployeeId = 1,
            LeaveTypeId = 1,
            FromDate = DateTime.Today.AddDays(20),
            ToDate = DateTime.Today.AddDays(30),
            Reason = "Long holiday",
            Status = "Pending",
            AppliedDate = DateTime.Today
        };

        context.LeaveRequests.Add(leaveRequest);
        context.SaveChanges();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ApproveLeaveRequestAsync(2, new LeaveActionDto { ApprovedBy = "Manager", Comments = "Approved" }));

        Assert.Contains("Insufficient leave balance", ex.Message);
    }
}
