namespace System.Challenge.FIAP.Services;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Challenge.FIAP.Configuration;
using System.Challenge.FIAP.Entities;
using System.Challenge.FIAP.Interface;

public sealed class JwtService : IJwtService
{
    private readonly JwtSettings _jwtSettings;
    private readonly TokenValidationParameters _tokenValidationParameters;
    private readonly JwtSecurityTokenHandler _tokenHandler;

    public JwtService(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
        _jwtSettings.Validate();
        _tokenHandler = new JwtSecurityTokenHandler();
        
        _tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)),
            ValidateIssuer = true,
            ValidIssuer = _jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = _jwtSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            RequireExpirationTime = true,
            RequireSignedTokens = true
        };
    }

    public Task<string> GenerateToken(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var claims = BuildClaims(user);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.Add(_jwtSettings.TokenExpiry),
            signingCredentials: credentials,
            notBefore: DateTime.UtcNow
        );

        return Task.FromResult(_tokenHandler.WriteToken(token));
    }

    public async Task<string> GetUserFromTokenAsync(string token)
    {
        var principal = await ValidateAndGetPrincipalAsync(token);
        return principal.FindFirst(ClaimTypes.Email)?.Value
            ?? throw new InvalidOperationException("Token não contém email");
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            await ValidateAndGetPrincipalAsync(token);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public Task<ClaimsPrincipal?> GetPrincipalFromTokenAsync(string token)
    {
        try
        {
            var principal = ValidateAndGetPrincipalAsync(token).GetAwaiter().GetResult();
            return Task.FromResult<ClaimsPrincipal?>(principal);
        }
        catch
        {
            return Task.FromResult<ClaimsPrincipal?>(null);
        }
    }

    public Task<string> GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Task.FromResult(Convert.ToBase64String(randomNumber));
    }

    public Task<DateTime> GetTokenExpirationAsync(string token)
    {
        var normalizedToken = NormalizeToken(token);
        var jwtToken = _tokenHandler.ReadJwtToken(normalizedToken);
        return Task.FromResult(jwtToken.ValidTo);
    }

    private Task<ClaimsPrincipal> ValidateAndGetPrincipalAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token inválido", nameof(token));

        var normalizedToken = NormalizeToken(token);

        try
        {
            var principal = _tokenHandler.ValidateToken(
                normalizedToken, 
                _tokenValidationParameters, 
                out SecurityToken validatedToken);

            if (validatedToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Algoritmo inválido");
            }

            return Task.FromResult(principal);
        }
        catch (SecurityTokenExpiredException)
        {
            throw new UnauthorizedAccessException("Token expirado");
        }
        catch (SecurityTokenInvalidSignatureException)
        {
            throw new UnauthorizedAccessException("Assinatura inválida");
        }
        catch (SecurityTokenException ex)
        {
            throw new UnauthorizedAccessException($"Token inválido: {ex.Message}");
        }
    }

    private static string NormalizeToken(string token) 
        => token.Replace("Bearer ", string.Empty, StringComparison.OrdinalIgnoreCase).Trim();

    private static List<Claim> BuildClaims(User user) => new()
    {
        new(JwtRegisteredClaimNames.Sub, user.IdUser.ToString()),
        new(JwtRegisteredClaimNames.Email, user.Email),
        new(JwtRegisteredClaimNames.Name, user.FullName),
        new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
        new(ClaimTypes.NameIdentifier, user.IdUser.ToString()),
        new(ClaimTypes.Email, user.Email),
        new(ClaimTypes.Role, user.Role),
        new("Document", user.Document)
    };
}