using HireFlow.DTOs.Application;

namespace HireFlow.Services.Application
{
    public interface IApplicationService
    {
        // Candidate applies for a job
        Task<ApplicationResponseDto> ApplyAsync(
            int jobId,
            int candidateId,
            ApplyJobRequestDto request);

        // Candidate gets own applications
        Task<List<ApplicationResponseDto>> GetMyApplicationsAsync(
            int candidateId);

        // Recruiter gets applications for a job (checks job ownership)
        Task<List<ApplicationResponseDto>> GetJobApplicationsAsync(
            int jobId,
            int recruiterId);

        // Recruiter gets all applications across their posted jobs
        Task<List<ApplicationResponseDto>> GetRecruiterApplicationsAsync(
            int recruiterId);

        // Recruiter updates application status (checks job ownership)
        Task<bool> UpdateStatusAsync(
            int applicationId,
            int recruiterId,
            string status);
    }
}