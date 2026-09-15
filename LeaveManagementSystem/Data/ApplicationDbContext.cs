using LeaveManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
namespace LeaveManagementSystem.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Employee> Employees { get; set; }

    public DbSet<LeaveType> LeaveTypes { get; set; }

    public DbSet<LeaveRequest> LeaveRequests { get; set; }

    public DbSet<LeaveBalance> LeaveBalances { get; set; }

    public DbSet<User> Users { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


        // =========================
        // Seed Leave Types
        // =========================

        modelBuilder.Entity<LeaveType>().HasData(

            new LeaveType
            {
                LeaveTypeId = 1,
                LeaveTypeName = "Annual Leave",
                MaximumDays = 15,
                IsActive = true
            },

            new LeaveType
            {
                LeaveTypeId = 2,
                LeaveTypeName = "Sick Leave",
                MaximumDays = 10,
                IsActive = true
            },

            new LeaveType
            {
                LeaveTypeId = 3,
                LeaveTypeName = "Casual Leave",
                MaximumDays = 5,
                IsActive = true
            }
        );


        // =========================
        // Seed Employees
        // =========================

        modelBuilder.Entity<Employee>().HasData(

            new Employee
            {
                EmployeeId = 1,
                EmployeeCode = "EMP001",
                Name = "Priya",
                Email = "priya@example.com",
                Department = "IT",
                JoiningDate = new DateTime(2026, 1, 15),
                IsActive = true
            },

            new Employee
            {
                EmployeeId = 2,
                EmployeeCode = "EMP002",
                Name = "Poojitha",
                Email = "poojitha@example.com",
                Department = "HR",
                JoiningDate = new DateTime(2026, 3, 10),
                IsActive = true
            },

            new Employee
            {
                EmployeeId = 3,
                EmployeeCode = "EMP003",
                Name = "Arjun",
                Email = "arjun@example.com",
                Department = "Finance",
                JoiningDate = new DateTime(2026, 6, 20),
                IsActive = true
            },

            new Employee
            {
                EmployeeId = 4,
                EmployeeCode = "EMP004",
                Name = "Saritha",
                Email = "saritha@example.com",
                Department = "Administration",
                JoiningDate = new DateTime(2026, 2, 5),
                IsActive = true
            }
        );


        // =========================
        // Seed Leave Balances
        // =========================

        modelBuilder.Entity<LeaveBalance>().HasData(

            // Priya
            new LeaveBalance
            {
                LeaveBalanceId = 1,
                EmployeeId = 1,
                LeaveTypeId = 1,
                TotalDays = 15,
                UsedDays = 0
            },
            new LeaveBalance
            {
                LeaveBalanceId = 2,
                EmployeeId = 1,
                LeaveTypeId = 2,
                TotalDays = 10,
                UsedDays = 0
            },
            new LeaveBalance
            {
                LeaveBalanceId = 3,
                EmployeeId = 1,
                LeaveTypeId = 3,
                TotalDays = 12,
                UsedDays = 0
            },


            // Poojitha
            new LeaveBalance
            {
                LeaveBalanceId = 4,
                EmployeeId = 2,
                LeaveTypeId = 1,
                TotalDays = 15,
                UsedDays = 0
            },
            new LeaveBalance
            {
                LeaveBalanceId = 5,
                EmployeeId = 2,
                LeaveTypeId = 2,
                TotalDays = 10,
                UsedDays = 0
            },
            new LeaveBalance
            {
                LeaveBalanceId = 6,
                EmployeeId = 2,
                LeaveTypeId = 3,
                TotalDays = 12,
                UsedDays = 0
            },


            // Arjun
            new LeaveBalance
            {
                LeaveBalanceId = 7,
                EmployeeId = 3,
                LeaveTypeId = 1,
                TotalDays = 15,
                UsedDays = 0
            },
            new LeaveBalance
            {
                LeaveBalanceId = 8,
                EmployeeId = 3,
                LeaveTypeId = 2,
                TotalDays = 10,
                UsedDays = 0
            },
            new LeaveBalance
            {
                LeaveBalanceId = 9,
                EmployeeId = 3,
                LeaveTypeId = 3,
                TotalDays = 12,
                UsedDays = 0
            },


            // Saritha
            new LeaveBalance
            {
                LeaveBalanceId = 10,
                EmployeeId = 4,
                LeaveTypeId = 1,
                TotalDays = 15,
                UsedDays = 0
            },
            new LeaveBalance
            {
                LeaveBalanceId = 11,
                EmployeeId = 4,
                LeaveTypeId = 2,
                TotalDays = 10,
                UsedDays = 0
            },
            new LeaveBalance
            {
                LeaveBalanceId = 12,
                EmployeeId = 4,
                LeaveTypeId = 3,
                TotalDays = 12,
                UsedDays = 0
            }

        );
        modelBuilder.Entity<User>()
               .HasOne(u => u.Employee)
               .WithMany()
               .HasForeignKey(u => u.EmployeeId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}