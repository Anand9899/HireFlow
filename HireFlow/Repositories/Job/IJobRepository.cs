using HireFlow.Models;

namespace HireFlow.Repositories.Job
{
    public interface IJobRepository
    {
        Task<List<Models.Job>> GetAllAsync();

        Task<List<Models.Job>> GetByRecruiterIdAsync(int recruiterId);

        Task<Models.Job?> GetByIdAsync(int id);

        Task<Models.Job> CreateAsync(Models.Job job);

        Task<bool> UpdateAsync(Models.Job job);

        Task<bool> DeleteAsync(int id);

        Task<Dictionary<int, int>> GetApplicantsCountsAsync(IEnumerable<int> jobIds);
    }
}