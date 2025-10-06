namespace System.Challenge.FIAP.DTOs;

public record EnrollmentDto
{
    public Guid IdEnrollment { get; set; }
    public Guid IdStudent { get; set; }
    public Guid IdCourse { get; set; }
    public Guid? IdClass { get; set; }
    public DateTime EnrollmentDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string Status { get; set; } = "Active";
    
    public string? StudentName { get; set; }
    public string? CourseName { get; set; }
    public string? ClassName { get; set; }
}

