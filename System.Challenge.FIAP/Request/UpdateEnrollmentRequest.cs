using System.ComponentModel.DataAnnotations;

namespace System.Challenge.FIAP.Request;

public record UpdateEnrollmentRequest
{
    [StringLength(20)]
    public string? Status { get; init; }
}

