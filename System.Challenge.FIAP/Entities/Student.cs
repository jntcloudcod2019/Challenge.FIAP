using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace System.Challenge.FIAP.Entities;

[Table("Students")]
public record Student
{
    [Key]
    public Guid IdStudent { get; init; }

    [Required]
    [ForeignKey(nameof(User))]
    public Guid IdUser { get; init; }

    [Required]
    [StringLength(20)]
    public string RegistrationNumber { get; init; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string FullName { get; init; } = string.Empty;

    [Required]
    [StringLength(14)]
    public string Cpf { get; init; } = string.Empty;

    public DateTime? BirthDate { get; init; }

    [StringLength(200)]
    public string? Address { get; init; }

    [StringLength(15)]
    public string? PhoneNumber { get; init; }

    [Required]
    public DateTime CreatedAt { get; init; }

    public DateTime? UpdatedAt { get; init; }

    public User User { get; init; } = null!;
    public ICollection<Enrollment> Enrollments { get; init; } = new List<Enrollment>();
}

