using System.ComponentModel.DataAnnotations;

namespace LeaveManagementSystem.DTOs;

public class CreateLeaveRequestDto
{
    [Range(1, int.MaxValue)]
    public int EmployeeId { get; set; }

    [Range(1, int.MaxValue)]
    public int LeaveTypeId { get; set; }

    public DateTime FromDate { get; set; }

    public DateTime ToDate { get; set; }

    [Required, StringLength(500)]
    public string Reason { get; set; } = string.Empty;
}