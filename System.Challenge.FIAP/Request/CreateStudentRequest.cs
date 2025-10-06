using System.ComponentModel.DataAnnotations;

namespace System.Challenge.FIAP.Request;

public record CreateStudentRequest
{
    [Required(ErrorMessage = "RA (Registro Acadêmico) é obrigatório")]
    [StringLength(20, MinimumLength = 5, ErrorMessage = "RA deve ter entre 5 e 20 caracteres")]
    public string RegistrationNumber { get; init; } = string.Empty;

    [Required(ErrorMessage = "Nome completo é obrigatório")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Nome completo deve ter entre 3 e 200 caracteres")]
    public string FullName { get; init; } = string.Empty;

    [Required(ErrorMessage = "CPF é obrigatório")]
    [StringLength(14, MinimumLength = 11, ErrorMessage = "CPF deve ter entre 11 e 14 caracteres")]
    [RegularExpression(@"^\d{3}\.?\d{3}\.?\d{3}-?\d{2}$", ErrorMessage = "Formato de CPF inválido")]
    public string Cpf { get; init; } = string.Empty;

    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Formato de email inválido")]
    public string Email { get; init; } = string.Empty;

    [DataType(DataType.Date)]
    public DateTime? BirthDate { get; init; }

    [StringLength(200, ErrorMessage = "Endereço deve ter no máximo 200 caracteres")]
    public string? Address { get; init; }

    [StringLength(15, MinimumLength = 10, ErrorMessage = "Telefone deve ter entre 10 e 15 caracteres")]
    [Phone(ErrorMessage = "Formato de telefone inválido")]
    public string? PhoneNumber { get; init; }
}

