using System.Challenge.FIAP.DTOs;

namespace System.Challenge.FIAP.Response;

public record AuthResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? Token { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public UserDto? User { get; init; }
}

