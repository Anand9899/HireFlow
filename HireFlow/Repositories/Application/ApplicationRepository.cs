using HireFlow.Data;
using HireFlow.Models;
using Microsoft.EntityFrameworkCore;

namespace HireFlow.Repositories.Application
{
    public class ApplicationRepository : IApplicationRepository
    {
        private readonly ApplicationDbContext _context;

        public ApplicationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // Get Application By ID
        // =====================================================

        public async Task<JobApplication?> GetByIdAsync(int id)
        {
            return await _context.JobApplications
                .Include(a => a.Job)
                .Include(a => a.Candidate)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        // =====================================================
        // Get Applications By Candidate ID
        // =====================================================

        public async Task<List<JobApplication>> GetByCandidateIdAsync(
            int candidateId)
        {
            return await _context.JobApplications
                .Include(a => a.Job)
                .Include(a => a.Candidate)
                .Where(a => a.CandidateId == candidateId)
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();
        }

        // =====================================================
        // Get Applications By Job ID
        // =====================================================

        public async Task<List<JobApplication>> GetByJobIdAsync(
            int jobId)
        {
            return await _context.JobApplications
                .Include(a => a.Job)
                .Include(a => a.Candidate)
                .Where(a => a.JobId == jobId)
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();
        }

        // =====================================================
        // Get Applications Across All Jobs of a Recruiter
        // =====================================================

        public async Task<List<JobApplication>> GetRecruiterApplicationsAsync(
            int recruiterId)
        {
            return await _context.JobApplications
                .Include(a => a.Job)
                .Include(a => a.Candidate)
                .Where(a => a.Job != null && a.Job.RecruiterId == recruiterId)
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();
        }

        // =====================================================
        // Create Application
        // =====================================================

        public async Task<JobApplication> CreateAsync(
            JobApplication application)
        {
            _context.JobApplications.Add(application);

            await _context.SaveChangesAsync();

            return application;
        }

        // =====================================================
        // Update Application Status
        // =====================================================

        public async Task<bool> UpdateStatusAsync(
            int id,
            string status)
        {
            var application = await _context.JobApplications
                .FirstOrDefaultAsync(a => a.Id == id);

            if (application == null)
            {
                return false;
            }

            application.Status = status;
            application.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        // =====================================================
        // Check Existing Application
        // =====================================================

        public async Task<bool> HasAppliedAsync(
            int jobId,
            int candidateId)
        {
            return await _context.JobApplications
                .AnyAsync(a =>
                    a.JobId == jobId &&
                    a.CandidateId == candidateId);
        }
    }
}