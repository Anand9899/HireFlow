using System.Security.Claims;
using HireFlow.DTOs.Job;
using HireFlow.Services.Job;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HireFlow.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobController : ControllerBase
    {
        private readonly IJobService _jobService;

        public JobController(IJobService jobService)
        {
            _jobService = jobService;
        }

        // =====================================================
        // GET: /api/Job
        // Get all jobs
        // =====================================================

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllJobs()
        {
            var jobs = await _jobService.GetAllAsync();

            return Ok(jobs);
        }

        // =====================================================
        // GET: /api/Job/my-jobs
        // Get all jobs posted by the logged-in recruiter
        // =====================================================

        [HttpGet("my-jobs")]
        [Authorize(Roles = "Recruiter")]
        public async Task<IActionResult> GetMyJobs()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out int recruiterId))
            {
                return Unauthorized(new
                {
                    message = "Invalid user identity."
                });
            }

            var jobs = await _jobService.GetByRecruiterIdAsync(recruiterId);

            return Ok(jobs);
        }

        // =====================================================
        // GET: /api/Job/{id}
        // Get job by ID
        // =====================================================

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetJobById(int id)
        {
            var job = await _jobService.GetByIdAsync(id);

            if (job == null)
            {
                return NotFound(new
                {
                    message = "Job not found."
                });
            }

            return Ok(job);
        }

        // =====================================================
        // POST: /api/Job
        // Create new job
        // Recruiter only
        // =====================================================

        [HttpPost]
        [Authorize(Roles = "Recruiter")]
        public async Task<IActionResult> CreateJob(
            CreateJobRequestDto request)
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out int recruiterId))
            {
                return Unauthorized(new
                {
                    message = "Invalid user identity."
                });
            }

            var job = await _jobService.CreateAsync(
                request,
                recruiterId);

            return CreatedAtAction(
                nameof(GetJobById),
                new { id = job.Id },
                job);
        }

        // =====================================================
        // PUT: /api/Job/{id}
        // Update job
        // Recruiter only
        // =====================================================

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Recruiter")]
        public async Task<IActionResult> UpdateJob(
            int id,
            UpdateJobRequestDto request)
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out int recruiterId))
            {
                return Unauthorized(new
                {
                    message = "Invalid user identity."
                });
            }

            var updated = await _jobService.UpdateAsync(
                id,
                request,
                recruiterId);

            if (!updated)
            {
                return NotFound(new
                {
                    message = "Job not found or you are not the owner of this job."
                });
            }

            return Ok(new
            {
                message = "Job updated successfully."
            });
        }

        // =====================================================
        // DELETE: /api/Job/{id}
        // Delete job
        // Recruiter only
        // =====================================================

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Recruiter")]
        public async Task<IActionResult> DeleteJob(int id)
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out int recruiterId))
            {
                return Unauthorized(new
                {
                    message = "Invalid user identity."
                });
            }

            var deleted = await _jobService.DeleteAsync(
                id,
                recruiterId);

            if (!deleted)
            {
                return NotFound(new
                {
                    message = "Job not found or you are not the owner of this job."
                });
            }

            return Ok(new
            {
                message = "Job deleted successfully."
            });
        }
    }
}