using System.Challenge.FIAP.Entities;
using System.Security.Claims;

namespace System.Challenge.FIAP.Interface;

public interface IJwtService
{
    Task<string> GenerateToken(User user);
    Task<string> GetUserFromTokenAsync(string token);
    Task<bool> ValidateTokenAsync(string token);
    Task<ClaimsPrincipal?> GetPrincipalFromTokenAsync(string token);
    Task<string> GenerateRefreshToken();
    Task<DateTime> GetTokenExpirationAsync(string token);
}