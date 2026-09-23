namespace HireFlow.DTOs.Application
{
    public class ApplicationResponseDto
    {
        public int Id { get; set; }

        public int JobId { get; set; }

        public string JobTitle { get; set; } = string.Empty;

        public int CandidateId { get; set; }

        public string CandidateName { get; set; } = string.Empty;

        public string CandidateEmail { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        // Structured Candidate Profile Information
        public string? FirstName { get; set; }

        public string? MiddleName { get; set; }

        public string? LastName { get; set; }

        public string? Email { get; set; }

        public string? Mobile { get; set; }

        public string? Gender { get; set; }

        public string? ProfileDomain { get; set; }

        public string? CurrentJob { get; set; }

        public string? CurrentLocation { get; set; }

        public string? HigherQualification { get; set; }

        public double? Marks10th { get; set; }

        public double? Marks12th { get; set; }

        public double? MarksUG { get; set; }

        public double? MarksPG { get; set; }

        public string? Resume { get; set; }

        public string? CoverLetter { get; set; }

        public DateTime AppliedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}