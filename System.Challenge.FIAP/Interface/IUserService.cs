using System.Challenge.FIAP.Request;
using System.Challenge.FIAP.Response;
using System.Challenge.FIAP.DTOs;

namespace System.Challenge.FIAP.Interface;

public interface IUserService
{
    Task<ApiResponse<UserDto>> CreateUserAsync(CreateUserRequest request);
    Task<(UserDto User, string Password)> CreateStudentUserAsync(CreateUserRequest request);
    Task<UserDto?> GetUserByIdAsync(Guid userId);
    Task<UserDto?> UpdateUserAsync(Guid userId, UpdateUserRequest request);
    Task<bool> DeleteUserAsync(Guid userId);
    Task<List<UserDto>> GetAllUsersAsync();
    Task<List<UserDto>> SearchUsersAsync(string query);
    string GenerateStrongPasswordForStudent();
}