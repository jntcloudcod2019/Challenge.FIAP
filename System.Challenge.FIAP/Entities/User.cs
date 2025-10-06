using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace System.Challenge.FIAP.Entities;

[Table("Users")]
public record User
{
    [Key]
    public Guid IdUser { get; init; }

    [Required]
    [StringLength(200)]
    public string FullName { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(200)]
    public string Email { get; init; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string Password { get; init; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Document { get; init; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Role { get; init; } = string.Empty;

    [Required]
    public bool StatusAccount { get; init; }

    [Required]
    public DateTime CreatedAt { get; init; }

    public DateTime? UpdatedAt { get; init; }

    public Student? Student { get; init; }
}