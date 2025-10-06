using System.ComponentModel.DataAnnotations;

namespace System.Challenge.FIAP.Request;

public record CreateClassRequest
{
    [Required(ErrorMessage = "Nome da turma é obrigatório")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Nome da turma deve ter entre 3 e 100 caracteres")]
    public string Name { get; init; } = string.Empty;

    [StringLength(500, ErrorMessage = "Descrição deve ter no máximo 500 caracteres")]
    public string? Description { get; init; }

    [StringLength(100, ErrorMessage = "Email ou documento do professor deve ter no máximo 100 caracteres")]
    public string? ProfessorEmailOrDocument { get; init; }

    [StringLength(50, ErrorMessage = "Sala deve ter no máximo 50 caracteres")]
    public string? Room { get; init; }
}

