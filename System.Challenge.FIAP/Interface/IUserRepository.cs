using System.Challenge.FIAP.Entities;

namespace System.Challenge.FIAP.Interface;

public interface IUserRepository
{
    Task<User> CreateUserAsync(User user);
    Task<User?> GetUserByIdAsync(Guid userId);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByDocumentAsync(string document);
    Task<bool> UserExistsAsync(string email);
    Task<User> UpdateUserAsync(User user);
    Task<bool> DeleteUserAsync(Guid userId);
    Task<List<User>> GetAllUsersAsync();
    Task<List<User>> SearchUsersAsync(string query);
}