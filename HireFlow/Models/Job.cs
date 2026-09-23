using System.ComponentModel.DataAnnotations;

namespace HireFlow.Models
{
    public class Job
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Location { get; set; } = string.Empty;

        [MaxLength(50)]
        public string JobType { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Salary { get; set; } = string.Empty;

        public int RecruiterId { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        public User? Recruiter { get; set; }
    }
}