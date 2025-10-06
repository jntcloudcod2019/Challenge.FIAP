using System.ComponentModel.DataAnnotations;

namespace System.Challenge.FIAP.Request;

public record UpdateUserRequest
{
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Nome deve ter entre 3 e 200 caracteres")]
    public string? FullName { get; init; }

    [EmailAddress(ErrorMessage = "Email inválido")]
    [StringLength(200, ErrorMessage = "Email deve ter no máximo 200 caracteres")]
    public string? Email { get; init; }

    [StringLength(20, ErrorMessage = "Documento deve ter no máximo 20 caracteres")]
    public string? Document { get; init; }

    [StringLength(50, ErrorMessage = "Role deve ter no máximo 50 caracteres")]
    public string? Role { get; init; }

    public bool? StatusAccount { get; init; }

    [StringLength(100, MinimumLength = 6, ErrorMessage = "Senha deve ter entre 6 e 100 caracteres")]
    public string? Password { get; init; }
}

