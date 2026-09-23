using HireFlow.DTOs.Auth;

namespace HireFlow.Services.Auth
{
    public interface IAuthService
    {
        Task<string> RegisterAsync(RegisterRequestDto request);

        Task<string> LoginAsync(LoginRequestDto request);
    }
}