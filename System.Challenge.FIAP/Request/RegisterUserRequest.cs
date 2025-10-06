using System.ComponentModel.DataAnnotations;

namespace System.Challenge.FIAP.Request;

public record RegisterUserRequest
{
    [Required(ErrorMessage = "Nome completo é obrigatório")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Nome deve ter entre 3 e 200 caracteres")]
    public string FullName { get; init; } = string.Empty;

    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "Senha é obrigatória")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Senha deve ter no mínimo 6 caracteres")]
    public string Password { get; init; } = string.Empty;

    [Required(ErrorMessage = "Documento é obrigatório")]
    [StringLength(20, ErrorMessage = "Documento deve ter no máximo 20 caracteres")]
    public string Document { get; init; } = string.Empty;

    [Required(ErrorMessage = "Role é obrigatório")]
    [RegularExpression("^(Admin|User)$", ErrorMessage = "Role deve ser 'Admin' ou 'User'")]
    public string Role { get; init; } = "User";
}
