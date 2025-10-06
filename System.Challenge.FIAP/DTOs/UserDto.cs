namespace System.Challenge.FIAP.DTOs;

public record UserDto
{
    public Guid IdUser { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Document { get; set; }
    public string Role { get; set; }
    public bool StatusAccount { get; set; }

}