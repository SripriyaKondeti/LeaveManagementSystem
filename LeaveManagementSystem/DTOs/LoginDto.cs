using System.ComponentModel.DataAnnotations;

namespace LeaveManagementSystem.DTOs
{
    public class LoginDto
    {
        [Required, StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required, StringLength(200)]
        public string Password { get; set; } = string.Empty;
    }
}