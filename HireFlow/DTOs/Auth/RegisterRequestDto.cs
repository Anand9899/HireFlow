using System.ComponentModel.DataAnnotations;

namespace HireFlow.DTOs.Auth
{
    public class RegisterRequestDto
    {
        // =========================
        // Full Name
        // =========================

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;


        // =========================
        // Email
        // =========================

        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;


        // =========================
        // Password
        // =========================

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;


        // =========================
        // Confirm Password
        // =========================

        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; } = string.Empty;


        // =========================
        // Account Type
        // Candidate / Recruiter
        // =========================

        [Required]
        public string AccountType { get; set; } = string.Empty;
    }
}