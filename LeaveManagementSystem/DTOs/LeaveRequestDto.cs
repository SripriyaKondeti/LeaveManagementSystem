namespace LeaveManagementSystem.DTOs;

public class LeaveRequestDto
{
    public int LeaveRequestId { get; set; }

    public int EmployeeId { get; set; }

    public int LeaveTypeId { get; set; }

    public DateTime FromDate { get; set; }

    public DateTime ToDate { get; set; }

    public string Reason { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime AppliedDate { get; set; }

    public string? ApprovedBy { get; set; }

    public string? Comments { get; set; }
}