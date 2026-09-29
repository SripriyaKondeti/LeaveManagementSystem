using LeaveManagementSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagementSystem.Data;

public static class DbSeeder
{
    public static async Task SeedUsersAsync(
        ApplicationDbContext context)
    {
        // Make sure the database is created and migrations are applied
        await context.Database.MigrateAsync();

        // Seed users only if they do not already exist
        if (!await context.Users.AnyAsync())
        {
            var passwordHasher = new PasswordHasher<User>();

            var priya = new User
            {
                Username = "priya",
                Role = "Employee",
                EmployeeId = 1
            };

            priya.PasswordHash = passwordHasher.HashPassword(
                priya,
                "Priya@123");

            var poojitha = new User
            {
                Username = "poojitha",
                Role = "Employee",
                EmployeeId = 2
            };

            poojitha.PasswordHash = passwordHasher.HashPassword(
                poojitha,
                "Poojitha@123");

            var arjun = new User
            {
                Username = "arjun",
                Role = "Employee",
                EmployeeId = 3
            };

            arjun.PasswordHash = passwordHasher.HashPassword(
                arjun,
                "Arjun@123");

            var sai = new User
            {
                Username = "sai",
                Role = "Manager",
                EmployeeId = 5
            };

            sai.PasswordHash = passwordHasher.HashPassword(
                sai,
                "Sai@123");

            context.Users.AddRange(
                priya,
                poojitha,
                arjun,
                sai);

            await context.SaveChangesAsync();
        }

        // Make sure all active employees have balances
        // for all active leave types.
        var activeEmployees = await context.Employees
            .Where(e => e.IsActive)
            .ToListAsync();

        var activeLeaveTypes = await context.LeaveTypes
            .Where(l => l.IsActive)
            .ToListAsync();

        var existingBalances = await context.LeaveBalances
            .ToListAsync();

        var newBalances = new List<LeaveBalance>();

        foreach (var employee in activeEmployees)
        {
            foreach (var leaveType in activeLeaveTypes)
            {
                var balanceExists = existingBalances.Any(b =>
                    b.EmployeeId == employee.EmployeeId &&
                    b.LeaveTypeId == leaveType.LeaveTypeId);

                if (!balanceExists)
                {
                    newBalances.Add(new LeaveBalance
                    {
                        EmployeeId = employee.EmployeeId,
                        LeaveTypeId = leaveType.LeaveTypeId,
                        TotalDays = leaveType.MaximumDays,
                        UsedDays = 0
                    });
                }
            }
        }

        if (newBalances.Any())
        {
            context.LeaveBalances.AddRange(newBalances);

            await context.SaveChangesAsync();
        }
    }
}