using System.ComponentModel.DataAnnotations;

namespace System.Challenge.FIAP.Request;

public record UpdateClassRequest
{
    [Required(ErrorMessage = "Código da turma é obrigatório")]
    [StringLength(20, MinimumLength = 2, ErrorMessage = "Código da turma deve ter entre 2 e 20 caracteres")]
    public string ClassCode { get; init; } = string.Empty;

    [StringLength(100, ErrorMessage = "Email ou documento do professor deve ter no máximo 100 caracteres")]
    public string? ProfessorEmailOrDocument { get; init; }

    [StringLength(50, ErrorMessage = "Sala deve ter no máximo 50 caracteres")]
    public string? Room { get; init; }

    [StringLength(20, ErrorMessage = "Status deve ter no máximo 20 caracteres")]
    [RegularExpression("^(Open|Closed|Cancelled)$", ErrorMessage = "Status deve ser: Open, Closed ou Cancelled")]
    public string? Status { get; init; }
}

