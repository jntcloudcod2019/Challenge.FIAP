using System.ComponentModel.DataAnnotations;

namespace System.Challenge.FIAP.Request;

public record CreateEnrollmentRequest
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string StudentDocumentOrRA { get; init; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string ClassCode { get; init; } = string.Empty;

    [StringLength(20)]
    public string Status { get; init; } = "Active";
}

