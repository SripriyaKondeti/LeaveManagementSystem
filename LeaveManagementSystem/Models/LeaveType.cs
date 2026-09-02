namespace LeaveManagementSystem.Models;

public class LeaveType
{
    public int LeaveTypeId { get; set; }

    public string LeaveTypeName { get; set; } = string.Empty;

    public int MaximumDays { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation Properties
    public ICollection<LeaveRequest> LeaveRequests { get; set; }
        = new List<LeaveRequest>();

    public ICollection<LeaveBalance> LeaveBalances { get; set; }
        = new List<LeaveBalance>();
}
