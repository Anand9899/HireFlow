using System.ComponentModel.DataAnnotations;

namespace HireFlow.DTOs.Job
{
    public class UpdateJobRequestDto
    {
        [Required]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Location { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string JobType { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Salary { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}