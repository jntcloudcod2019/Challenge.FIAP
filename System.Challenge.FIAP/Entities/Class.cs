using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace System.Challenge.FIAP.Entities;

[Table("Classes")]
public record Class
{
    [Key]
    public Guid IdClass { get; init; }

    public Guid? IdProfessor { get; init; }

    [Required]
    [StringLength(20)]
    public string ClassCode { get; init; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Name { get; init; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; init; }

    public int Capacity { get; init; } = 50;

    [Required]
    public DateTime CreatedAt { get; init; }

    public DateTime? UpdatedAt { get; init; }

    [StringLength(50)]
    public string? Room { get; init; }

    [Required]
    [StringLength(20)]
    public string Status { get; init; } = "Open";

    public ICollection<Enrollment> Enrollments { get; init; } = new List<Enrollment>();
}

