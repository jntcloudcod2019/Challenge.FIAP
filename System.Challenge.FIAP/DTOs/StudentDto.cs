namespace System.Challenge.FIAP.DTOs;

public record StudentDto
{
    public Guid IdStudent { get; set; }
    public Guid IdUser { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public DateTime? BirthDate { get; set; }
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public string? UserFullName { get; set; }
    public string? UserEmail { get; set; }
    public string? UserDocument { get; set; }
    public bool? UserStatus { get; set; }
    
    public int TotalEnrollments { get; set; }
    public int ActiveEnrollments { get; set; }
}

