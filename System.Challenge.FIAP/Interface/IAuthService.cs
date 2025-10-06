using System.Challenge.FIAP.Request;
using System.Challenge.FIAP.Response;
using System.Challenge.FIAP.DTOs;

namespace System.Challenge.FIAP.Interface;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterUserRequest request);
    Task<AuthResponse> LoginAsync(LoginUserRequest request);
    Task<UserDto?> GetCurrentUserAsync(string email);
}
