using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace System.Challenge.FIAP.Entities;

[Table("Enrollments")]
public record Enrollment
{
    [Key]
    public Guid IdEnrollment { get; init; }

    [Required]
    [ForeignKey(nameof(Student))]
    public Guid IdStudent { get; init; }
    [ForeignKey(nameof(Class))]
    public Guid? IdClass { get; init; }

    [Required]
    public DateTime EnrollmentDate { get; init; }

    [Required]
    public DateTime CreatedAt { get; init; }

    public DateTime? UpdatedAt { get; init; }

    [Required]
    [StringLength(20)]
    public string Status { get; init; } = "Active";

    public Student Student { get; init; } = null!;
    public Class? Class { get; init; }

}

