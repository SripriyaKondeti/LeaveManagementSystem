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

        // Do not create users if they already exist
        if (await context.Users.AnyAsync())
        {
            return;
        }

        var passwordHasher = new PasswordHasher<User>();

        var priya = new User
        {
            Username = "priya",
            Role = "Employee",
            EmployeeId = 1,
            IsActive = true
        };

        priya.PasswordHash = passwordHasher.HashPassword(
            priya,
            "Priya@123");

        var poojitha = new User
        {
            Username = "poojitha",
            Role = "Employee",
            EmployeeId = 2,
            IsActive = true
        };

        poojitha.PasswordHash = passwordHasher.HashPassword(
            poojitha,
            "Poojitha@123");

        var arjun = new User
        {
            Username = "arjun",
            Role = "Employee",
            EmployeeId = 3,
            IsActive = true
        };

        arjun.PasswordHash = passwordHasher.HashPassword(
            arjun,
            "Arjun@123");

        var sai = new User
        {
            Username = "sai",
            Role = "Manager",
            EmployeeId = 5,
            IsActive = true
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
}