namespace LeaveManagementSystem.DTOs;

public class LeaveTypeDto
{
    public int LeaveTypeId { get; set; }

    public string LeaveTypeName { get; set; } = string.Empty;

    public int MaximumDays { get; set; }

    public bool IsActive { get; set; }
}