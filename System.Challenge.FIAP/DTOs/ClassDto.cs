namespace System.Challenge.FIAP.DTOs;

public record ClassDto
{
    public Guid IdClass { get; set; }
    public Guid? IdProfessor { get; set; }
    public string ClassCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Capacity { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? Room { get; set; }
    public string Status { get; set; } = "Open";
    
    public string? ProfessorName { get; set; }
    public string? ProfessorEmail { get; set; }
    
    public int TotalEnrollments { get; set; }
    public int AvailableSeats { get; set; }
}

