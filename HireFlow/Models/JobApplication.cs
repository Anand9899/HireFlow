using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HireFlow.Models
{
    public class JobApplication
    {
        [Key]
        public int Id { get; set; }

        // Job Reference
        [Required]
        public int JobId { get; set; }

        // Candidate / User Reference
        [Required]
        public int CandidateId { get; set; }

        // Application Status
        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = "Pending";

        // Personal Information
        [MaxLength(60)]
        public string? FirstName { get; set; }

        [MaxLength(60)]
        public string? MiddleName { get; set; }

        [MaxLength(60)]
        public string? LastName { get; set; }

        [MaxLength(120)]
        public string? Email { get; set; }

        [MaxLength(30)]
        public string? Mobile { get; set; }

        [MaxLength(30)]
        public string? Gender { get; set; }

        // Professional Details
        [MaxLength(100)]
        public string? ProfileDomain { get; set; }

        [MaxLength(120)]
        public string? CurrentJob { get; set; }

        [MaxLength(120)]
        public string? CurrentLocation { get; set; }

        // Academic Qualifications & Marks
        [MaxLength(120)]
        public string? HigherQualification { get; set; }

        public double? Marks10th { get; set; }

        public double? Marks12th { get; set; }

        public double? MarksUG { get; set; }

        public double? MarksPG { get; set; }

        // Resume & Cover Letter
        [MaxLength(255)]
        public string? Resume { get; set; }

        public string? CoverLetter { get; set; }

        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(JobId))]
        public Job? Job { get; set; }

        [ForeignKey(nameof(CandidateId))]
        public User? Candidate { get; set; }
    }
}