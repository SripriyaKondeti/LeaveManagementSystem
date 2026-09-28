using System.ComponentModel.DataAnnotations;

namespace LeaveManagementSystem.DTOs;

public class LeaveTypeDto
{
    public int LeaveTypeId { get; set; }

    [Required, StringLength(100)]
    public string LeaveTypeName { get; set; } = string.Empty;

    [Range(1, 366)]
    public int MaximumDays { get; set; }

    public bool IsActive { get; set; }
}