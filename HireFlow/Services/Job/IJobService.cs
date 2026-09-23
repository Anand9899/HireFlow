using HireFlow.DTOs.Job;

namespace HireFlow.Services.Job
{
    public interface IJobService
    {
        Task<List<JobResponseDto>> GetAllAsync();

        Task<List<JobResponseDto>> GetByRecruiterIdAsync(int recruiterId);

        Task<JobResponseDto?> GetByIdAsync(int id);

        Task<JobResponseDto> CreateAsync(
            CreateJobRequestDto request,
            int recruiterId);

        Task<bool> UpdateAsync(
            int id,
            UpdateJobRequestDto request,
            int recruiterId);

        Task<bool> DeleteAsync(
            int id,
            int recruiterId);
    }
}