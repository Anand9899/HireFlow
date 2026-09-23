using System.Security.Claims;
using HireFlow.DTOs.Application;
using HireFlow.Services.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HireFlow.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ApplicationController : ControllerBase
    {
        private readonly IApplicationService _applicationService;

        public ApplicationController(
            IApplicationService applicationService)
        {
            _applicationService = applicationService;
        }

        // =====================================================
        // POST: /api/Application/apply/{jobId}
        // Candidate applies for a job
        // =====================================================

        [HttpPost("apply/{jobId:int}")]
        [Authorize(Roles = "Candidate")]
        public async Task<IActionResult> ApplyForJob(
            int jobId,
            ApplyJobRequestDto request)
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out int candidateId))
            {
                return Unauthorized(new
                {
                    message = "Invalid user identity."
                });
            }

            try
            {
                var application =
                    await _applicationService.ApplyAsync(
                        jobId,
                        candidateId,
                        request);

                return Ok(new
                {
                    message = "Job application submitted successfully.",
                    application
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        // =====================================================
        // GET: /api/Application/my-applications
        // Candidate views own applications
        // =====================================================

        [HttpGet("my-applications")]
        [Authorize(Roles = "Candidate")]
        public async Task<IActionResult> GetMyApplications()
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out int candidateId))
            {
                return Unauthorized(new
                {
                    message = "Invalid user identity."
                });
            }

            var applications =
                await _applicationService
                    .GetMyApplicationsAsync(candidateId);

            return Ok(applications);
        }

        // =====================================================
        // GET: /api/Application/job/{jobId}
        // Recruiter views applications for a job (authorized only for owner)
        // =====================================================

        [HttpGet("job/{jobId:int}")]
        [Authorize(Roles = "Recruiter")]
        public async Task<IActionResult> GetJobApplications(
            int jobId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out int recruiterId))
            {
                return Unauthorized(new
                {
                    message = "Invalid user identity."
                });
            }

            try
            {
                var applications =
                    await _applicationService
                        .GetJobApplicationsAsync(jobId, recruiterId);

                return Ok(applications);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    message = ex.Message
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    message = ex.Message
                });
            }
        }

        // =====================================================
        // GET: /api/Application/recruiter-applications
        // Recruiter views all applications across their posted jobs
        // =====================================================

        [HttpGet("recruiter-applications")]
        [Authorize(Roles = "Recruiter")]
        public async Task<IActionResult> GetRecruiterApplications()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out int recruiterId))
            {
                return Unauthorized(new
                {
                    message = "Invalid user identity."
                });
            }

            var applications =
                await _applicationService
                    .GetRecruiterApplicationsAsync(recruiterId);

            return Ok(applications);
        }

        // =====================================================
        // PUT: /api/Application/{applicationId}/status
        // Recruiter updates application status
        // =====================================================

        [HttpPut("{applicationId:int}/status")]
        [Authorize(Roles = "Recruiter")]
        public async Task<IActionResult> UpdateApplicationStatus(
            int applicationId,
            UpdateApplicationStatusDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userId, out int recruiterId))
            {
                return Unauthorized(new
                {
                    message = "Invalid user identity."
                });
            }

            try
            {
                var updated =
                    await _applicationService.UpdateStatusAsync(
                        applicationId,
                        recruiterId,
                        request.Status);

                if (!updated)
                {
                    return NotFound(new
                    {
                        message = "Application not found."
                    });
                }

                return Ok(new
                {
                    message = "Application status updated successfully."
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    message = ex.Message
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }
    }
}