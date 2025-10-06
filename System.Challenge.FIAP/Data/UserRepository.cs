using Microsoft.EntityFrameworkCore;
using System.Challenge.FIAP.Entities;
using System.Challenge.FIAP.Interface;
using Microsoft.Data.SqlClient;

namespace System.Challenge.FIAP.Data;

public class UserRepository : IUserRepository
{
   private readonly IDbContextFactory<ContextDB> _contextFactory;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(IDbContextFactory<ContextDB> contextFactory, ILogger<UserRepository> logger)
   {
      _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            return await context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Erro de SQL ao buscar usuário por email: {Email}", email);
            throw new InvalidOperationException($"Erro ao acessar banco de dados: {ex.Message}", ex);
        }
    }

    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            return await context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.IdUser == userId);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Erro de SQL ao buscar usuário por ID: {UserId}", userId);
            throw new InvalidOperationException($"Erro ao acessar banco de dados: {ex.Message}", ex);
        }
    }

    public async Task<User?> GetUserByDocumentAsync(string document)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            return await context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Document == document);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Erro de SQL ao buscar usuário por documento: {Document}", document);
            throw new InvalidOperationException($"Erro ao acessar banco de dados: {ex.Message}", ex);
        }
    }

    public async Task<User> CreateUserAsync(User user)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            context.Users.Add(user);
            await context.SaveChangesAsync();
            
            _logger.LogInformation("Usuário criado com sucesso: {Email}", user.Email);
            return user;
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Erro de SQL ao criar usuário: {Email}", user.Email);
            throw new InvalidOperationException($"Erro ao acessar banco de dados: {ex.Message}", ex);
        }
        
    }

    public async Task<User> UpdateUserAsync(User user)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            context.Users.Update(user);
            await context.SaveChangesAsync();
            
            _logger.LogInformation("Usuário atualizado com sucesso: {UserId}", user.IdUser);
            return user;
        }
       
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Erro ao atualizar usuário no banco: {UserId}", user.IdUser);
            throw new InvalidOperationException($"Erro ao atualizar usuário: {ex.InnerException?.Message ?? ex.Message}", ex);
        }
    }

    public async Task<bool> DeleteUserAsync(Guid userId)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            var user = await context.Users.FindAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Tentativa de deletar usuário inexistente: {UserId}", userId);
                return false;
            }

            context.Users.Remove(user);
            await context.SaveChangesAsync();
            
            _logger.LogInformation("Usuário deletado com sucesso: {UserId}", userId);
            return true;
        }
        
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Erro de SQL ao deletar usuário: {UserId}", userId);
            throw new InvalidOperationException($"Erro ao acessar banco de dados: {ex.Message}", ex);
        }
       
    }

    public async Task<bool> UserExistsAsync(string email)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            return await context.Users
                .AnyAsync(u => u.Email == email);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Erro de SQL ao verificar existência de usuário: {Email}", email);
            throw new InvalidOperationException($"Erro ao acessar banco de dados: {ex.Message}", ex);
        }
        
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            return await context.Users
                .AsNoTracking()
                .OrderBy(u => u.FullName)
                .ToListAsync();
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Erro de SQL ao listar todos os usuários");
            throw new InvalidOperationException($"Erro ao acessar banco de dados: {ex.Message}", ex);
        }
       
    }

    public async Task<List<User>> GetAllUsersPaginatedAsync(int pageNumber, int pageSize)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            var skip = (pageNumber - 1) * pageSize;
            
            return await context.Users
                .AsNoTracking()
                .OrderBy(u => u.FullName)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync()
                .ConfigureAwait(false);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Erro de SQL ao buscar usuários paginados");
            throw new InvalidOperationException($"Erro ao acessar banco de dados: {ex.Message}", ex);
        }
       
    }

    public async Task<int> GetTotalUsersCountAsync()
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            return await context.Users
                .AsNoTracking()
                .CountAsync()
                .ConfigureAwait(false);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Erro de SQL ao contar usuários");
            throw new InvalidOperationException($"Erro ao acessar banco de dados: {ex.Message}", ex);
        }
       
    }

    public async Task<List<User>> SearchUsersAsync(string query)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            return await context.Users
                .AsNoTracking()
                .Where(u =>
                    u.FullName.Contains(query) ||
                    u.Email.Contains(query) ||
                    u.Document.Contains(query))
                .OrderBy(u => u.FullName)
                .ToListAsync()
                .ConfigureAwait(false);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Erro de SQL ao buscar usuários");
            throw new InvalidOperationException($"Erro ao acessar banco de dados: {ex.Message}", ex);
        }
        
    }

    public async Task<List<User>> SearchUsersPaginatedAsync(string query, int pageNumber, int pageSize)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            var skip = (pageNumber - 1) * pageSize;
            
            return await context.Users
                .AsNoTracking()
                .Where(u =>
                    u.FullName.Contains(query) ||
                    u.Email.Contains(query) ||
                    u.Document.Contains(query))
                .OrderBy(u => u.FullName)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync()
                .ConfigureAwait(false);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Erro de SQL ao buscar usuários paginados");
            throw new InvalidOperationException($"Erro ao acessar banco de dados: {ex.Message}", ex);
        }
       
    }

    public async Task<int> GetSearchUsersCountAsync(string query)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            return await context.Users
                .AsNoTracking()
                .Where(u =>
                    u.FullName.Contains(query) ||
                    u.Email.Contains(query) ||
                    u.Document.Contains(query))
                .CountAsync()
                .ConfigureAwait(false);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Erro de SQL ao contar usuários na busca");
            throw new InvalidOperationException($"Erro ao acessar banco de dados: {ex.Message}", ex);
        }
       
    }
}