namespace LeaveManagementSystem.Models;

public class LeaveRequest
{
    public int LeaveRequestId { get; set; }

    // Foreign Key
    public int EmployeeId { get; set; }

    // Foreign Key
    public int LeaveTypeId { get; set; }

    public DateTime FromDate { get; set; }

    public DateTime ToDate { get; set; }

    public string Reason { get; set; } = string.Empty;

    public string Status { get; set; } = "Pending";

    public DateTime AppliedDate { get; set; }

    public string? ApprovedBy { get; set; }

    public string? Comments { get; set; }


    // Navigation Properties
    public Employee Employee { get; set; } = null!;

    public LeaveType LeaveType { get; set; } = null!;
}
