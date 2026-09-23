using HireFlow.DTOs.Auth;
using HireFlow.Services.Auth;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HireFlow.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }


        // =====================================================
        // REGISTER
        // POST: /api/Auth/register
        // =====================================================

        [HttpPost("register")]
        public async Task<IActionResult> Register(
            [FromBody] RegisterRequestDto request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new
                    {
                        message = "Registration data is required."
                    });
                }


                var result =
                    await _authService.RegisterAsync(request);


                // ---------------------------------------------
                // Email already registered
                // ---------------------------------------------

                if (result == "Email is already registered.")
                {
                    return BadRequest(new
                    {
                        message = result
                    });
                }


                // ---------------------------------------------
                // Invalid account type
                // ---------------------------------------------

                if (result == "Invalid account type.")
                {
                    return BadRequest(new
                    {
                        message = result
                    });
                }


                // ---------------------------------------------
                // Successful registration
                // ---------------------------------------------

                return Ok(new
                {
                    success = true,
                    message = result
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "Register Error: " + ex.Message
                );

                return StatusCode(500, new
                {
                    message = "An error occurred during registration."
                });
            }
        }


        // =====================================================
        // LOGIN
        // POST: /api/Auth/login
        // =====================================================

        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequestDto request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new
                    {
                        message = "Login data is required."
                    });
                }


                var result =
                    await _authService.LoginAsync(request);


                // ---------------------------------------------
                // Invalid email/password
                // ---------------------------------------------

                if (result == "Invalid email or password.")
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = "Invalid email or password."
                    });
                }


                // ---------------------------------------------
                // Inactive account
                // ---------------------------------------------

                if (result == "Your account is inactive.")
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = "Your account is inactive."
                    });
                }


                // ---------------------------------------------
                // Token not generated
                // ---------------------------------------------

                if (string.IsNullOrWhiteSpace(result))
                {
                    return StatusCode(500, new
                    {
                        success = false,
                        message = "Login failed. Token could not be generated."
                    });
                }


                // ---------------------------------------------
                // Successful login
                // ---------------------------------------------

                return Ok(new
                {
                    success = true,
                    message = "Login successful.",
                    token = result
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "Login Error: " + ex.Message
                );

                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred during login."
                });
            }
        }


        // =====================================================
        // VALIDATE LOGIN TOKEN
        // GET: /api/Auth/validate
        // =====================================================

        [Authorize]
        [HttpGet("validate")]
        public IActionResult ValidateToken()
        {
            return Ok(new
            {
                success = true,
                valid = true,
                message = "Token is valid."
            });
        }


        // =====================================================
        // CHECK CURRENT USER
        // GET: /api/Auth/me
        // =====================================================

        [Authorize]
        [HttpGet("me")]
        public IActionResult GetCurrentUser()
        {
            var userId =
                User.FindFirst(
                    System.Security.Claims.ClaimTypes.NameIdentifier
                )?.Value;

            var email =
                User.FindFirst(
                    System.Security.Claims.ClaimTypes.Email
                )?.Value;

            var name =
                User.FindFirst(
                    System.Security.Claims.ClaimTypes.Name
                )?.Value;

            var role =
                User.FindFirst(
                    System.Security.Claims.ClaimTypes.Role
                )?.Value;

            return Ok(new
            {
                success = true,
                userId = userId,
                name = name,
                email = email,
                role = role
            });
        }


        // =====================================================
        // LOGOUT
        // POST: /api/Auth/logout
        // =====================================================

        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            return Ok(new
            {
                success = true,
                message = "Logout successful."
            });
        }
    }
}