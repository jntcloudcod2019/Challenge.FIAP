namespace System.Challenge.FIAP.Configuration;

public sealed class JwtSettings
{
    public const string SectionName = "JwtSettings";

    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationInMinutes { get; set; } = 240;

    public TimeSpan TokenExpiry => TimeSpan.FromMinutes(ExpirationInMinutes);

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(SecretKey))
            throw new InvalidOperationException("JWT SecretKey não configurada!");

        if (SecretKey.Length < 32)
            throw new InvalidOperationException("JWT SecretKey deve ter no mínimo 32 caracteres!");

        if (string.IsNullOrWhiteSpace(Issuer))
            throw new InvalidOperationException("JWT Issuer não configurado!");

        if (string.IsNullOrWhiteSpace(Audience))
            throw new InvalidOperationException("JWT Audience não configurado!");
    }
}
