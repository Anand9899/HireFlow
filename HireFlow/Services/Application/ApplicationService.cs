using HireFlow.DTOs.Application;
using HireFlow.Models;
using HireFlow.Repositories.Application;
using HireFlow.Repositories.Job;

namespace HireFlow.Services.Application
{
    public class ApplicationService : IApplicationService
    {
        private readonly IApplicationRepository _repository;
        private readonly IJobRepository _jobRepository;

        public ApplicationService(
            IApplicationRepository repository,
            IJobRepository jobRepository)
        {
            _repository = repository;
            _jobRepository = jobRepository;
        }

        // Candidate applies for a job
        public async Task<ApplicationResponseDto> ApplyAsync(
            int jobId,
            int candidateId,
            ApplyJobRequestDto request)
        {
            // Verify Job exists and is active
            var job = await _jobRepository.GetByIdAsync(jobId);
            if (job == null)
            {
                throw new KeyNotFoundException("Job not found.");
            }

            if (!job.IsActive)
            {
                throw new InvalidOperationException("This job is no longer accepting applications.");
            }

            if (job.RecruiterId == candidateId)
            {
                throw new InvalidOperationException("You cannot apply to your own job.");
            }

            // Check if candidate already applied
            var alreadyApplied = await _repository
                .HasAppliedAsync(jobId, candidateId);

            if (alreadyApplied)
            {
                throw new InvalidOperationException(
                    "You have already applied for this job.");
            }

            var application = new JobApplication
            {
                JobId = jobId,
                CandidateId = candidateId,
                Status = "Pending",
                FirstName = request.FirstName?.Trim(),
                MiddleName = request.MiddleName?.Trim(),
                LastName = request.LastName?.Trim(),
                Email = request.Email?.Trim(),
                Mobile = request.Mobile?.Trim(),
                Gender = request.Gender?.Trim(),
                ProfileDomain = request.ProfileDomain?.Trim(),
                CurrentJob = request.CurrentJob?.Trim(),
                CurrentLocation = request.CurrentLocation?.Trim(),
                HigherQualification = request.HigherQualification?.Trim(),
                Marks10th = request.Marks10th,
                Marks12th = request.Marks12th,
                MarksUG = request.MarksUG,
                MarksPG = request.MarksPG,
                Resume = request.Resume?.Trim(),
                CoverLetter = request.CoverLetter?.Trim(),
                AppliedAt = DateTime.UtcNow
            };

            var createdApplication =
                await _repository.CreateAsync(application);

            var result = await _repository
                .GetByIdAsync(createdApplication.Id);

            return MapToDto(result!);
        }

        // Get applications of logged-in candidate
        public async Task<List<ApplicationResponseDto>>
            GetMyApplicationsAsync(int candidateId)
        {
            var applications = await _repository
                .GetByCandidateIdAsync(candidateId);

            return applications
                .Select(MapToDto)
                .ToList();
        }

        // Get applications for a job (with recruiter ownership validation)
        public async Task<List<ApplicationResponseDto>>
            GetJobApplicationsAsync(int jobId, int recruiterId)
        {
            var job = await _jobRepository.GetByIdAsync(jobId);
            if (job == null)
            {
                throw new KeyNotFoundException("Job not found.");
            }

            if (job.RecruiterId != recruiterId)
            {
                throw new UnauthorizedAccessException("You are not authorized to view applications for this job.");
            }

            var applications = await _repository
                .GetByJobIdAsync(jobId);

            return applications
                .Select(MapToDto)
                .ToList();
        }

        // Get all applications across all jobs of a recruiter
        public async Task<List<ApplicationResponseDto>>
            GetRecruiterApplicationsAsync(int recruiterId)
        {
            var applications = await _repository
                .GetRecruiterApplicationsAsync(recruiterId);

            return applications
                .Select(MapToDto)
                .ToList();
        }

        // Update application status (with recruiter ownership validation)
        public async Task<bool> UpdateStatusAsync(
            int applicationId,
            int recruiterId,
            string status)
        {
            var application = await _repository.GetByIdAsync(applicationId);
            if (application == null)
            {
                return false;
            }

            if (application.Job == null || application.Job.RecruiterId != recruiterId)
            {
                throw new UnauthorizedAccessException("You are not authorized to update applications for this job.");
            }

            var validStatuses = new[]
            {
                "Pending",
                "Accepted",
                "Rejected"
            };

            if (!validStatuses.Contains(
                status,
                StringComparer.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    "Invalid application status. Must be Pending, Accepted, or Rejected.");
            }

            // Normalize casing
            var normalizedStatus = validStatuses.First(s => s.Equals(status, StringComparison.OrdinalIgnoreCase));

            return await _repository.UpdateStatusAsync(
                applicationId,
                normalizedStatus);
        }

        // Convert Entity to DTO
        private static ApplicationResponseDto MapToDto(
            JobApplication application)
        {
            var fullName = !string.IsNullOrWhiteSpace(application.FirstName)
                ? string.Join(" ", new[] { application.FirstName, application.MiddleName, application.LastName }.Where(s => !string.IsNullOrWhiteSpace(s)))
                : (application.Candidate?.FullName ?? "Applicant");

            return new ApplicationResponseDto
            {
                Id = application.Id,
                JobId = application.JobId,
                JobTitle = application.Job?.Title ?? "Untitled Job",
                CandidateId = application.CandidateId,
                CandidateName = fullName,
                CandidateEmail = application.Email ?? application.Candidate?.Email ?? string.Empty,
                Status = application.Status,
                FirstName = application.FirstName,
                MiddleName = application.MiddleName,
                LastName = application.LastName,
                Email = application.Email ?? application.Candidate?.Email,
                Mobile = application.Mobile,
                Gender = application.Gender,
                ProfileDomain = application.ProfileDomain,
                CurrentJob = application.CurrentJob,
                CurrentLocation = application.CurrentLocation,
                HigherQualification = application.HigherQualification,
                Marks10th = application.Marks10th,
                Marks12th = application.Marks12th,
                MarksUG = application.MarksUG,
                MarksPG = application.MarksPG,
                Resume = application.Resume,
                CoverLetter = application.CoverLetter,
                AppliedAt = application.AppliedAt,
                UpdatedAt = application.UpdatedAt
            };
        }
    }
}