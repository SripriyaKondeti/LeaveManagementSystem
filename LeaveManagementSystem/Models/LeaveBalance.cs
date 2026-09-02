namespace LeaveManagementSystem.Models;

public class LeaveBalance
{
    public int LeaveBalanceId { get; set; }

    // Foreign Keys
    public int EmployeeId { get; set; }

    public int LeaveTypeId { get; set; }

    public int TotalDays { get; set; }

    public int UsedDays { get; set; } = 0;


    // Navigation Properties
    public Employee Employee { get; set; } = null!;

    public LeaveType LeaveType { get; set; } = null!;
}
