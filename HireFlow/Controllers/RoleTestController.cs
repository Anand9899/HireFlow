using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HireFlow.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoleTestController : ControllerBase
    {
        [HttpGet("candidate")]
        [Authorize(Roles = "Candidate")]
        public IActionResult CandidateOnly()
        {
            return Ok(new
            {
                message = "Candidate access granted."
            });
        }

        [HttpGet("recruiter")]
        [Authorize(Roles = "Recruiter")]
        public IActionResult RecruiterOnly()
        {
            return Ok(new
            {
                message = "Recruiter access granted."
            });
        }

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public IActionResult AdminOnly()
        {
            return Ok(new
            {
                message = "Admin access granted."
            });
        }
    }
}