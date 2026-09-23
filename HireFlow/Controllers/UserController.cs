using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HireFlow.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        // ==========================================
        // GET: /api/User/profile
        // ==========================================

        [HttpGet("profile")]
        public IActionResult GetProfile()
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            var fullName = User.FindFirstValue(
                ClaimTypes.Name);

            var email = User.FindFirstValue(
                ClaimTypes.Email);

            var role = User.FindFirstValue(
                ClaimTypes.Role);

            return Ok(new
            {
                id = userId,
                fullName = fullName,
                email = email,
                role = role,
                message = "Authenticated user profile."
            });
        }
    }
}