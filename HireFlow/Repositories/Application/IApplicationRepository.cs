using HireFlow.Models;

namespace HireFlow.Repositories.Application
{
    public interface IApplicationRepository
    {
        // Get application by application ID
        Task<JobApplication?> GetByIdAsync(int id);

        // Get all applications submitted by a candidate
        Task<List<JobApplication>> GetByCandidateIdAsync(
            int candidateId);

        // Get all applications for a particular job
        Task<List<JobApplication>> GetByJobIdAsync(
            int jobId);

        // Get all applications across all jobs for a recruiter
        Task<List<JobApplication>> GetRecruiterApplicationsAsync(
            int recruiterId);

        // Create a new job application
        Task<JobApplication> CreateAsync(
            JobApplication application);

        // Update application status
        Task<bool> UpdateStatusAsync(
            int id,
            string status);

        // Check whether candidate has already applied for a job
        Task<bool> HasAppliedAsync(
            int jobId,
            int candidateId);
    }
}