using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using HireFlow.Data;
using HireFlow.DTOs.Auth;
using HireFlow.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace HireFlow.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(
            ApplicationDbContext context,
            IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }


        // =====================================================
        // REGISTER
        // =====================================================

        public async Task<string> RegisterAsync(
            RegisterRequestDto request)
        {
            // ---------------------------------------------
            // Check existing email
            // ---------------------------------------------

            var normalizedEmail = request.Email.Trim().ToLowerInvariant();

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(
                    u => u.Email == normalizedEmail);

            if (existingUser != null)
            {
                return "Email is already registered.";
            }


            // ---------------------------------------------
            // Validate Account Type
            // ---------------------------------------------

            var accountType =
                request.AccountType.Trim();

            if (
                !accountType.Equals(
                    "Candidate",
                    StringComparison.OrdinalIgnoreCase)
                &&
                !accountType.Equals(
                    "Recruiter",
                    StringComparison.OrdinalIgnoreCase)
            )
            {
                return "Invalid account type.";
            }


            // ---------------------------------------------
            // Normalize Role
            // ---------------------------------------------

            var role =
                accountType.Equals(
                    "Recruiter",
                    StringComparison.OrdinalIgnoreCase)
                    ? "Recruiter"
                    : "Candidate";


            // ---------------------------------------------
            // Create User
            // ---------------------------------------------

            var user = new User
            {
                FullName = request.FullName.Trim(),

                Email = normalizedEmail,

                PasswordHash =
                    BCrypt.Net.BCrypt.HashPassword(
                        request.Password),

                Role = role,

                IsActive = true,

                CreatedAt = DateTime.UtcNow
            };


            // ---------------------------------------------
            // Save User
            // ---------------------------------------------

            _context.Users.Add(user);

            await _context.SaveChangesAsync();


            return "Registration successful.";
        }


        // =====================================================
        // LOGIN
        // =====================================================

        public async Task<string> LoginAsync(
            LoginRequestDto request)
        {
            // ---------------------------------------------
            // Find user
            // ---------------------------------------------

            var normalizedEmail = request.Email.Trim().ToLowerInvariant();

            var user = await _context.Users
                .FirstOrDefaultAsync(
                    u => u.Email == normalizedEmail);

            if (user == null)
            {
                return "Invalid email or password.";
            }


            // ---------------------------------------------
            // Check active account
            // ---------------------------------------------

            if (!user.IsActive)
            {
                return "Your account is inactive.";
            }


            // ---------------------------------------------
            // Verify password
            // ---------------------------------------------

            var passwordValid =
                BCrypt.Net.BCrypt.Verify(
                    request.Password,
                    user.PasswordHash);

            if (!passwordValid)
            {
                return "Invalid email or password.";
            }


            // =================================================
            // JWT CLAIMS
            // =================================================

            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.Id.ToString()),

                new Claim(
                    ClaimTypes.Name,
                    user.FullName),

                new Claim(
                    ClaimTypes.Email,
                    user.Email),

                new Claim(
                    ClaimTypes.Role,
                    user.Role)
            };


            // =================================================
            // JWT KEY
            // =================================================

            var key =
                _configuration["Jwt:Key"];

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new InvalidOperationException(
                    "JWT Key is not configured.");
            }


            var securityKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(key));


            var credentials =
                new SigningCredentials(
                    securityKey,
                    SecurityAlgorithms.HmacSha256);


            // =================================================
            // JWT TOKEN
            // =================================================

            var expiryMinutes =
                double.TryParse(_configuration["Jwt:ExpiryMinutes"], out var parsedExp) && parsedExp > 0
                    ? parsedExp
                    : 60;


            var token =
                new JwtSecurityToken(
                    issuer:
                        _configuration[
                            "Jwt:Issuer"],

                    audience:
                        _configuration[
                            "Jwt:Audience"],

                    claims: claims,

                    expires:
                        DateTime.UtcNow.AddMinutes(
                            expiryMinutes),

                    signingCredentials:
                        credentials);


            // ---------------------------------------------
            // Return token
            // ---------------------------------------------

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }
    }
}