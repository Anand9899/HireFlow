using System.ComponentModel.DataAnnotations;

namespace HireFlow.Models
{
    public class User
    {
        public int Id 
        {
            get;
            set;
        }

        [Required]
        [MaxLength(100)]
        public string FullName 
        {
            get;
            set;
        } 
            = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string Email 
        {
            get;
            set; 
        } 
            = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Role
        { 
            get;
            set;
        }
            = "Candidate";

        public bool IsActive
        {
            get;
            set;
        } 
            = true;

        public DateTime CreatedAt 
        {
            get;
            set;
        } 
            = DateTime.UtcNow;

        public DateTime? UpdatedAt 
        {
            get;
            set;
        }
    }
}