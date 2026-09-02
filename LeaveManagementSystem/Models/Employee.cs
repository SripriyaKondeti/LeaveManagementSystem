namespace LeaveManagementSystem.Models;
public class Employee
{
    public int EmployeeId { get; set; }

    public string EmployeeCode { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Department { get; set; } = string.Empty;

    public DateTime JoiningDate { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation Properties
    public ICollection<LeaveRequest> LeaveRequests { get; set; }
        = new List<LeaveRequest>();

    public ICollection<LeaveBalance> LeaveBalances { get; set; }
        = new List<LeaveBalance>();
}