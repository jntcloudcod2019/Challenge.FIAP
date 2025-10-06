using System.Challenge.FIAP.Entities;
using System.Challenge.FIAP.Interface;
using System.Challenge.FIAP.Request;
using System.Challenge.FIAP.Response;
using System.Challenge.FIAP.DTOs;

namespace System.Challenge.FIAP.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepository userRepository, ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<ApiResponse<UserDto>> CreateUserAsync(CreateUserRequest request)
    {
        try
        {
            if (await _userRepository.UserExistsAsync(request.Email))
            {
                return ApiResponse<UserDto>.ErrorResponse("Email já cadastrado");
            }

            var existingDocument = await _userRepository.GetUserByDocumentAsync(request.Document);
            if (existingDocument != null)
            {
                return ApiResponse<UserDto>.ErrorResponse("Documento já cadastrado");
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
                StatusAccount = request.StatusAccount,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdUser = await _userRepository.CreateUserAsync(user);

            var userDto = new UserDto
            {
                IdUser = createdUser.IdUser,
                FullName = createdUser.FullName,
                Email = createdUser.Email,
                Document = createdUser.Document,
                Role = createdUser.Role,
                StatusAccount = createdUser.StatusAccount
            };

            _logger.LogInformation("Usuário criado: {Email}", createdUser.Email);

            return ApiResponse<UserDto>.SuccessResponse(userDto, "Usuário criado com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar usuário");
            return ApiResponse<UserDto>.ErrorResponse("Erro ao criar usuário", new List<string> { ex.Message });
        }
    }

    public async Task<(UserDto User, string Password)> CreateStudentUserAsync(CreateUserRequest request)
    {
        var generatedPassword = GenerateStrongPasswordForStudent();
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(generatedPassword);

        var user = new User
        {
            IdUser = Guid.NewGuid(),
            FullName = request.FullName,
            Email = request.Email,
            Document = request.Document,
            Role = "Student",
            StatusAccount = true,
            Password = hashedPassword,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdUser = await _userRepository.CreateUserAsync(user);

        var userDto = new UserDto
        {
            IdUser = createdUser.IdUser,
            FullName = createdUser.FullName,
            Email = createdUser.Email,
            Document = createdUser.Document,
            Role = createdUser.Role,
            StatusAccount = createdUser.StatusAccount
        };

        return (userDto, generatedPassword);
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid userId)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);

        if (user == null)
        {
            return null;
        }

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

    public async Task<UserDto?> UpdateUserAsync(Guid userId, UpdateUserRequest request)
    {
        var existingUser = await _userRepository.GetUserByIdAsync(userId);

        if (existingUser == null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(request.Email) && request.Email != existingUser.Email)
        {
            if (await _userRepository.UserExistsAsync(request.Email))
            {
                throw new InvalidOperationException("Email já cadastrado");
            }
        }

        if (!string.IsNullOrWhiteSpace(request.Document) && request.Document != existingUser.Document)
        {
            var existingDocument = await _userRepository.GetUserByDocumentAsync(request.Document);
            if (existingDocument != null)
            {
                throw new InvalidOperationException("Documento já cadastrado");
            }
        }

        var updatedUser = existingUser with
        {
            FullName = request.FullName ?? existingUser.FullName,
            Email = request.Email ?? existingUser.Email,
            Document = request.Document ?? existingUser.Document,
            Role = request.Role ?? existingUser.Role,
            StatusAccount = request.StatusAccount ?? existingUser.StatusAccount,
            Password = !string.IsNullOrWhiteSpace(request.Password) 
                ? BCrypt.Net.BCrypt.HashPassword(request.Password) 
                : existingUser.Password,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await _userRepository.UpdateUserAsync(updatedUser);

        return new UserDto
        {
            IdUser = result.IdUser,
            FullName = result.FullName,
            Email = result.Email,
            Document = result.Document,
            Role = result.Role,
            StatusAccount = result.StatusAccount
        };
    }

    public async Task<bool> DeleteUserAsync(Guid userId)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);

        if (user == null)
        {
            return false;
        }

        var result = await _userRepository.DeleteUserAsync(userId);

        if (result)
        {
            _logger.LogInformation("Usuário deletado: {UserId}", userId);
        }

        return result;
    }

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllUsersAsync();

        return users.Select(u => new UserDto
        {
            IdUser = u.IdUser,
            FullName = u.FullName,
            Email = u.Email,
            Document = u.Document,
            Role = u.Role,
            StatusAccount = u.StatusAccount
        }).ToList();
    }

    public async Task<List<UserDto>> SearchUsersAsync(string query)
    {
        var users = await _userRepository.SearchUsersAsync(query);

        return users.Select(u => new UserDto
        {
            IdUser = u.IdUser,
            FullName = u.FullName,
            Email = u.Email,
            Document = u.Document,
            Role = u.Role,
            StatusAccount = u.StatusAccount
        }).ToList();
    }

    public string GenerateStrongPasswordForStudent()
    {
        const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string lowercase = "abcdefghijklmnopqrstuvwxyz";
        const string digits = "0123456789";
        const string specialChars = "!@#$%^&*()_+-=[]{}|;:,.<>?";

        var random = new Random();
        var password = new char[12];

        password[0] = uppercase[random.Next(uppercase.Length)];
        password[1] = lowercase[random.Next(lowercase.Length)];
        password[2] = digits[random.Next(digits.Length)];
        password[3] = specialChars[random.Next(specialChars.Length)];

        var allChars = uppercase + lowercase + digits + specialChars;
        for (int i = 4; i < password.Length; i++)
        {
            password[i] = allChars[random.Next(allChars.Length)];
        }

        for (int i = 0; i < password.Length; i++)
        {
            int randomIndex = random.Next(password.Length);
            (password[i], password[randomIndex]) = (password[randomIndex], password[i]);
        }

        return new string(password);
    }

}

