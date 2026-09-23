using HireFlow.DTOs.Job;
using HireFlow.Repositories.Job;

namespace HireFlow.Services.Job
{
    public class JobService : IJobService
    {
        private readonly IJobRepository _jobRepository;

        public JobService(IJobRepository jobRepository)
        {
            _jobRepository = jobRepository;
        }

        public async Task<List<JobResponseDto>> GetAllAsync()
        {
            var jobs = await _jobRepository.GetAllAsync();
            var jobIds = jobs.Select(j => j.Id).ToList();
            var counts = await _jobRepository.GetApplicantsCountsAsync(jobIds);

            return jobs.Select(job => new JobResponseDto
            {
                Id = job.Id,
                Title = job.Title,
                Description = job.Description,
                Location = job.Location,
                JobType = job.JobType,
                Salary = job.Salary,
                RecruiterId = job.RecruiterId,
                RecruiterName = job.Recruiter?.FullName ?? "HireFlow Recruiter",
                IsActive = job.IsActive,
                ApplicantsCount = counts.TryGetValue(job.Id, out var count) ? count : 0,
                CreatedAt = job.CreatedAt,
                UpdatedAt = job.UpdatedAt
            }).ToList();
        }

        public async Task<List<JobResponseDto>> GetByRecruiterIdAsync(int recruiterId)
        {
            var jobs = await _jobRepository.GetByRecruiterIdAsync(recruiterId);
            var jobIds = jobs.Select(j => j.Id).ToList();
            var counts = await _jobRepository.GetApplicantsCountsAsync(jobIds);

            return jobs.Select(job => new JobResponseDto
            {
                Id = job.Id,
                Title = job.Title,
                Description = job.Description,
                Location = job.Location,
                JobType = job.JobType,
                Salary = job.Salary,
                RecruiterId = job.RecruiterId,
                RecruiterName = job.Recruiter?.FullName ?? "HireFlow Recruiter",
                IsActive = job.IsActive,
                ApplicantsCount = counts.TryGetValue(job.Id, out var count) ? count : 0,
                CreatedAt = job.CreatedAt,
                UpdatedAt = job.UpdatedAt
            }).ToList();
        }

        public async Task<JobResponseDto?> GetByIdAsync(int id)
        {
            var job = await _jobRepository.GetByIdAsync(id);

            if (job == null)
            {
                return null;
            }

            var counts = await _jobRepository.GetApplicantsCountsAsync(new[] { id });

            return new JobResponseDto
            {
                Id = job.Id,
                Title = job.Title,
                Description = job.Description,
                Location = job.Location,
                JobType = job.JobType,
                Salary = job.Salary,
                RecruiterId = job.RecruiterId,
                RecruiterName = job.Recruiter?.FullName ?? "HireFlow Recruiter",
                IsActive = job.IsActive,
                ApplicantsCount = counts.TryGetValue(job.Id, out var count) ? count : 0,
                CreatedAt = job.CreatedAt,
                UpdatedAt = job.UpdatedAt
            };
        }

        public async Task<JobResponseDto> CreateAsync(
            CreateJobRequestDto request,
            int recruiterId)
        {
            var job = new Models.Job
            {
                Title = request.Title.Trim(),
                Description = request.Description.Trim(),
                Location = request.Location.Trim(),
                JobType = request.JobType.Trim(),
                Salary = request.Salary.Trim(),
                RecruiterId = recruiterId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var createdJob = await _jobRepository.CreateAsync(job);

            // Re-fetch to include recruiter details
            var jobWithRecruiter = await _jobRepository.GetByIdAsync(createdJob.Id) ?? createdJob;

            return new JobResponseDto
            {
                Id = jobWithRecruiter.Id,
                Title = jobWithRecruiter.Title,
                Description = jobWithRecruiter.Description,
                Location = jobWithRecruiter.Location,
                JobType = jobWithRecruiter.JobType,
                Salary = jobWithRecruiter.Salary,
                RecruiterId = jobWithRecruiter.RecruiterId,
                RecruiterName = jobWithRecruiter.Recruiter?.FullName ?? "HireFlow Recruiter",
                IsActive = jobWithRecruiter.IsActive,
                ApplicantsCount = 0,
                CreatedAt = jobWithRecruiter.CreatedAt,
                UpdatedAt = jobWithRecruiter.UpdatedAt
            };
        }

        public async Task<bool> UpdateAsync(
            int id,
            UpdateJobRequestDto request,
            int recruiterId)
        {
            var job = await _jobRepository.GetByIdAsync(id);

            if (job == null || job.RecruiterId != recruiterId)
            {
                return false;
            }

            job.Title = request.Title.Trim();
            job.Description = request.Description.Trim();
            job.Location = request.Location.Trim();
            job.JobType = request.JobType.Trim();
            job.Salary = request.Salary.Trim();
            job.IsActive = request.IsActive;

            return await _jobRepository.UpdateAsync(job);
        }

        public async Task<bool> DeleteAsync(
            int id,
            int recruiterId)
        {
            var job = await _jobRepository.GetByIdAsync(id);

            if (job == null || job.RecruiterId != recruiterId)
            {
                return false;
            }

            return await _jobRepository.DeleteAsync(id);
        }
    }
}