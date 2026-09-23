using System.ComponentModel.DataAnnotations;

namespace HireFlow.DTOs.Application
{
    public class ApplyJobRequestDto
    {
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

        [MaxLength(100)]
        public string? ProfileDomain { get; set; }

        [MaxLength(120)]
        public string? CurrentJob { get; set; }

        [MaxLength(120)]
        public string? CurrentLocation { get; set; }

        [MaxLength(120)]
        public string? HigherQualification { get; set; }

        public double? Marks10th { get; set; }

        public double? Marks12th { get; set; }

        public double? MarksUG { get; set; }

        public double? MarksPG { get; set; }

        [MaxLength(255)]
        public string? Resume { get; set; }

        [MaxLength(4000)]
        public string? CoverLetter { get; set; }
    }
}