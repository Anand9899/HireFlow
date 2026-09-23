using HireFlow.Data;
using HireFlow.Models;
using Microsoft.EntityFrameworkCore;

namespace HireFlow.Repositories.Job
{
    public class JobRepository : IJobRepository
    {
        private readonly ApplicationDbContext _context;

        public JobRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get all jobs
        public async Task<List<Models.Job>> GetAllAsync()
        {
            return await _context.Jobs
                .Include(j => j.Recruiter)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();
        }

        // Get jobs by recruiter ID
        public async Task<List<Models.Job>> GetByRecruiterIdAsync(int recruiterId)
        {
            return await _context.Jobs
                .Include(j => j.Recruiter)
                .Where(j => j.RecruiterId == recruiterId)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();
        }

        // Get job by ID
        public async Task<Models.Job?> GetByIdAsync(int id)
        {
            return await _context.Jobs
                .Include(j => j.Recruiter)
                .FirstOrDefaultAsync(j => j.Id == id);
        }

        // Create job
        public async Task<Models.Job> CreateAsync(Models.Job job)
        {
            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();
            return job;
        }

        // Update job
        public async Task<bool> UpdateAsync(Models.Job job)
        {
            var trackedJob = _context.Jobs.Local.FirstOrDefault(j => j.Id == job.Id);
            if (trackedJob == null)
            {
                _context.Jobs.Update(job);
            }
            else if (!ReferenceEquals(trackedJob, job))
            {
                _context.Entry(trackedJob).CurrentValues.SetValues(job);
            }

            job.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        // Delete job
        public async Task<bool> DeleteAsync(int id)
        {
            var job = _context.Jobs.Local.FirstOrDefault(j => j.Id == id)
                      ?? await _context.Jobs.FirstOrDefaultAsync(j => j.Id == id);

            if (job == null)
            {
                return false;
            }

            _context.Jobs.Remove(job);
            await _context.SaveChangesAsync();
            return true;
        }

        // Get applicants counts grouped by jobId
        public async Task<Dictionary<int, int>> GetApplicantsCountsAsync(IEnumerable<int> jobIds)
        {
            var idList = jobIds.ToList();
            if (!idList.Any())
            {
                return new Dictionary<int, int>();
            }

            return await _context.JobApplications
                .Where(a => idList.Contains(a.JobId))
                .GroupBy(a => a.JobId)
                .Select(g => new { JobId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.JobId, x => x.Count);
        }
    }
}