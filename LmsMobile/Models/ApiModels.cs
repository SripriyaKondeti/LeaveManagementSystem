using System.Text.Json.Serialization;

namespace LmsMobile.Models;

public class ApiResponse<T>
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }
}

public class LoginRequest
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}

public class LoginResponse
{
    public string Token { get; set; } = "";
    public string Role { get; set; } = "";
}

public class Employee
{
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = "";
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Department { get; set; } = "";
    public DateTime JoiningDate { get; set; }
    public bool IsActive { get; set; }

    [JsonIgnore] public bool IsInactive => !IsActive;
    [JsonIgnore] public string Subtitle => $"{EmployeeCode} · {Department}";
    [JsonIgnore] public string StatusText => IsActive ? "Active" : "Inactive";
}

public class CreateEmployeeRequest : Employee
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string Role { get; set; } = "Employee";
}

public class LeaveType
{
    public int LeaveTypeId { get; set; }
    public string LeaveTypeName { get; set; } = "";
    public int MaximumDays { get; set; }
    public bool IsActive { get; set; }

    [JsonIgnore] public string Summary => $"Max {MaximumDays} days";
    [JsonIgnore] public string StatusText => IsActive ? "Active" : "Inactive";
}

public class LeaveBalance
{
    public string LeaveType { get; set; } = "";
    public int TotalDays { get; set; }
    public int UsedDays { get; set; }
    public int RemainingDays { get; set; }

    [JsonIgnore] public string OfTotal => $"of {TotalDays} days";
}

public class LeaveRequest
{
    public int LeaveRequestId { get; set; }
    public int EmployeeId { get; set; }
    public int LeaveTypeId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public string Reason { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTime AppliedDate { get; set; }
    public string? ApprovedBy { get; set; }
    public string? Comments { get; set; }

    [JsonIgnore] public string LeaveTypeName { get; set; } = "";
    [JsonIgnore] public string EmployeeName { get; set; } = "";
    [JsonIgnore] public int Days => (ToDate.Date - FromDate.Date).Days + 1;
    [JsonIgnore] public string DateRange => $"{FromDate:dd MMM} – {ToDate:dd MMM yyyy} ({Days} {(Days == 1 ? "day" : "days")})";
    [JsonIgnore] public bool HasComments => !string.IsNullOrWhiteSpace(Comments);
    [JsonIgnore] public string CommentText => $"Manager: {Comments}";
}

public class CreateLeaveRequest
{
    public int EmployeeId { get; set; }
    public int LeaveTypeId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public string Reason { get; set; } = "";
}

public class LeaveAction
{
    public string? Comments { get; set; }
}

public class LeaveSummary
{
    public int TotalEmployees { get; set; }
    public int ActiveEmployees { get; set; }
    public int PendingRequests { get; set; }
    public int ApprovedRequests { get; set; }
    public int RejectedRequests { get; set; }
    public int TotalLeavesTaken { get; set; }
}
