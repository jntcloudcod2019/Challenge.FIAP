using System.Text.RegularExpressions;

namespace System.Challenge.FIAP.Services;

public interface IPasswordService
{
    bool IsPasswordStrong(string password);
    string GenerateStrongPassword();
    string HashPassword(string password);
    bool VerifyPassword(string password, string hashedPassword);
}

public class PasswordService : IPasswordService
{
    private readonly ILogger<PasswordService> _logger;

    public PasswordService(ILogger<PasswordService> logger)
    {
        _logger = logger;
    }

    public bool IsPasswordStrong(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            return false;

        var hasLowerCase = Regex.IsMatch(password, @"[a-z]");
        var hasUpperCase = Regex.IsMatch(password, @"[A-Z]");
        var hasDigit = Regex.IsMatch(password, @"[0-9]");
        var hasSpecialChar = Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]");

        return hasLowerCase && hasUpperCase && hasDigit && hasSpecialChar;
    }

    public string GenerateStrongPassword()
    {
        const string lowerCase = "abcdefghijklmnopqrstuvwxyz";
        const string upperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string digits = "0123456789";
        const string specialChars = "!@#$%^&*()_+-=[]{}|;:,.<>?";

        var random = new Random();
        var password = new List<char>();

        password.Add(lowerCase[random.Next(lowerCase.Length)]);
        password.Add(upperCase[random.Next(upperCase.Length)]);
        password.Add(digits[random.Next(digits.Length)]);
        password.Add(specialChars[random.Next(specialChars.Length)]);

        var allChars = lowerCase + upperCase + digits + specialChars;
        for (int i = 4; i < 12; i++)
        {
            password.Add(allChars[random.Next(allChars.Length)]);
        }

        for (int i = password.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (password[i], password[j]) = (password[j], password[i]);
        }

        return new string(password.ToArray());
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}
