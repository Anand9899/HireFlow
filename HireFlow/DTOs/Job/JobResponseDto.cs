namespace HireFlow.DTOs.Job
{
    public class JobResponseDto
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public string JobType { get; set; } = string.Empty;

        public string Salary { get; set; } = string.Empty;

        public int RecruiterId { get; set; }

        public string RecruiterName { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public int ApplicantsCount { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}