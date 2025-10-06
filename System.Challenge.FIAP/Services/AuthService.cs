using System.Challenge.FIAP.Entities;
using System.Challenge.FIAP.Interface;
using System.Challenge.FIAP.Request;
using System.Challenge.FIAP.Response;
using System.Challenge.FIAP.DTOs;

namespace System.Challenge.FIAP.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IJwtService jwtService,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterUserRequest request)
    {
        try
        {
            if (await _userRepository.UserExistsAsync(request.Email))
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Email já cadastrado"
                };
            }

            var existingDocument = await _userRepository.GetUserByDocumentAsync(request.Document);
            if (existingDocument != null)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Documento já cadastrado"
                };
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                IdUser = Guid.NewGuid(),
                FullName = request.FullName,
                Email = request.Email,
                Password = passwordHash,
                Document = request.Document,
                Role = request.Role,
                StatusAccount = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdUser = await _userRepository.CreateUserAsync(user);

            var token = await _jwtService.GenerateToken(createdUser);
            var expiresAt = await _jwtService.GetTokenExpirationAsync(token);

            _logger.LogInformation("Usuário registrado com sucesso: {Email}", request.Email);

            return new AuthResponse
            {
                Success = true,
                Message = "Usuário registrado com sucesso",
                Token = token,
                ExpiresAt = expiresAt,
                User = new UserDto
                {
                    IdUser = createdUser.IdUser,
                    FullName = createdUser.FullName,
                    Email = createdUser.Email,
                    Document = createdUser.Document,
                    Role = createdUser.Role,
                    StatusAccount = createdUser.StatusAccount
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao registrar usuário: {Email}", request.Email);
            return new AuthResponse
            {
                Success = false,
                Message = "Erro ao registrar usuário"
            };
        }
    }

    public async Task<AuthResponse> LoginAsync(LoginUserRequest request)
    {
        try
        {
            var user = await _userRepository.GetUserByEmailAsync(request.Email);
            
            if (user == null)
            {
                _logger.LogWarning("Tentativa de login com email não cadastrado: {Email}", request.Email);
                return new AuthResponse
                {
                    Success = false,
                    Message = "Email ou senha inválidos"
                };
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                _logger.LogWarning("Tentativa de login com senha incorreta: {Email}", request.Email);
                return new AuthResponse
                {
                    Success = false,
                    Message = "Email ou senha inválidos"
                };
            }

            if (!user.StatusAccount)
            {
                return new AuthResponse
                {
                    Success = false,
                    Message = "Conta inativa. Entre em contato com o administrador"
                };
            }

            var token = await _jwtService.GenerateToken(user);
            var expiresAt = await _jwtService.GetTokenExpirationAsync(token);

            _logger.LogInformation("Login realizado com sucesso: {Email}", request.Email);

            return new AuthResponse
            {
                Success = true,
                Message = "Login realizado com sucesso",
                Token = token,
                ExpiresAt = expiresAt,
                User = new UserDto
                {
                    IdUser = user.IdUser,
                    FullName = user.FullName,
                    Email = user.Email,
                    Document = user.Document,
                    Role = user.Role,
                    StatusAccount = user.StatusAccount
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao realizar login: {Email}", request.Email);
            return new AuthResponse
            {
                Success = false,
                Message = "Erro ao realizar login"
            };
        }
    }

    public async Task<UserDto?> GetCurrentUserAsync(string email)
    {
        var user = await _userRepository.GetUserByEmailAsync(email);
        
        if (user == null)
            return null;

        return new UserDto
        {
            IdUser = user.IdUser,
            FullName = user.FullName,
            Email = user.Email,
            Document = user.Document,
            Role = user.Role,
            StatusAccount = user.StatusAccount
        };
    }
}