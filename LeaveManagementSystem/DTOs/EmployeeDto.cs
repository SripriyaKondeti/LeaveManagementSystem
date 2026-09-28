using System.ComponentModel.DataAnnotations;

namespace LeaveManagementSystem.DTOs;

public class EmployeeDto
{
    public int EmployeeId { get; set; }

    [Required, StringLength(50)]
    public string EmployeeCode { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string Department { get; set; } = string.Empty;

    public DateTime JoiningDate { get; set; }

    public bool IsActive { get; set; }
}
