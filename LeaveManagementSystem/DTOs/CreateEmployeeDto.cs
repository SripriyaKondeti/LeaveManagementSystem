using System.ComponentModel.DataAnnotations;

namespace LeaveManagementSystem.DTOs;

public class CreateEmployeeDto : EmployeeDto
{
    [Required, StringLength(50, MinimumLength = 3)]
    [RegularExpression(@"^[a-zA-Z0-9._-]+$",
        ErrorMessage = "Username can contain only letters, digits, '.', '_' and '-'.")]
    public string Username { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 8)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).+$",
        ErrorMessage = "Password must contain an uppercase letter, a lowercase letter, a digit and a special character.")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^(Employee|Manager)$",
        ErrorMessage = "Role must be either 'Employee' or 'Manager'.")]
    public string Role { get; set; } = string.Empty;
}
