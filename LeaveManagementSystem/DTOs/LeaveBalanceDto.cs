namespace LeaveManagementSystem.DTOs;

public class LeaveBalanceDto
{
    public string LeaveType { get; set; } = string.Empty;

    public int TotalDays { get; set; }

    public int UsedDays { get; set; }

    public int RemainingDays { get; set; }
}