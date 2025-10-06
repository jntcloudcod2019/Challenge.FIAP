using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Challenge.FIAP.Entities;
using System.Challenge.FIAP.Interface;

namespace System.Challenge.FIAP.Data;

public class ClassRepository : IClassRepository
{
    private readonly IDbContextFactory<ContextDB> _contextFactory;
    private readonly ILogger<ClassRepository> _logger;

    public ClassRepository(IDbContextFactory<ContextDB> contextFactory, ILogger<ClassRepository> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<Class> CreateClassAsync(Class classEntity)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            context.Classes.Add(classEntity);
            await context.SaveChangesAsync().ConfigureAwait(false);

            return classEntity;
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Erro de SQL ao criar turma: {ClassCode}", classEntity.ClassCode);
            throw new InvalidOperationException($"Erro ao acessar banco de dados: {ex.Message}", ex);
        }
    }

    public async Task<List<Class>> GetAllClassesAsync()
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            return await context.Classes
                .AsNoTracking()
                .Include(c => c.Enrollments)
                .OrderBy(c => c.ClassCode)
                .ToListAsync()
                .ConfigureAwait(false);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Erro de SQL ao buscar todas as turmas");
            throw new InvalidOperationException($"Erro ao acessar banco de dados: {ex.Message}", ex);
        }
    }

    public async Task<List<Class>> SearchClassesAsync(string query)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            return await context.Classes
                .AsNoTracking()
                .Include(c => c.Enrollments)
                .Where(c => 
                    c.ClassCode.Contains(query) ||
                    c.Name.Contains(query) ||
                    c.Description.Contains(query) ||
                    c.Status.Contains(query))
                .OrderBy(c => c.ClassCode)
                .ToListAsync()
                .ConfigureAwait(false);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Erro de SQL ao buscar turmas");
            throw new InvalidOperationException($"Erro ao acessar banco de dados: {ex.Message}", ex);
        }
    }

    public async Task<Class> UpdateClassAsync(Class classEntity)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            context.Classes.Update(classEntity);
            await context.SaveChangesAsync().ConfigureAwait(false);

            return classEntity;
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Erro de SQL ao atualizar turma: {IdClass}", classEntity.IdClass);
            throw new InvalidOperationException($"Erro ao acessar banco de dados: {ex.Message}", ex);
        }
    }

    public async Task<bool> DeleteClassAsync(Guid id)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            var classEntity = await context.Classes.FindAsync(id);
            
            if (classEntity == null)
            {
                return false;
            }

            context.Classes.Remove(classEntity);
            await context.SaveChangesAsync().ConfigureAwait(false);

            return true;
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Erro de SQL ao excluir turma: {IdClass}", id);
            throw new InvalidOperationException($"Erro ao acessar banco de dados: {ex.Message}", ex);
        }
    }

    public async Task<Class?> GetClassByIdAsync(Guid id)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            return await context.Classes
                .AsNoTracking()
                .Include(c => c.Enrollments)
                .FirstOrDefaultAsync(c => c.IdClass == id)
                .ConfigureAwait(false);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Erro de SQL ao buscar turma por ID: {Id}", id);
            throw new InvalidOperationException($"Erro ao acessar banco de dados: {ex.Message}", ex);
        }
    }

    public async Task<Class?> GetClassByCodeAsync(string classCode)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            return await context.Classes
                .AsNoTracking()
                .Include(c => c.Enrollments)
                .FirstOrDefaultAsync(c => c.ClassCode == classCode)
                .ConfigureAwait(false);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Erro de SQL ao buscar turma por código: {ClassCode}", classCode);
            throw new InvalidOperationException($"Erro ao acessar banco de dados: {ex.Message}", ex);
        }
    }

    public async Task<bool> ClassCodeExistsAsync(string classCode)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            return await context.Classes
                .AsNoTracking()
                .AnyAsync(c => c.ClassCode == classCode)
                .ConfigureAwait(false);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Erro de SQL ao verificar existência do código da turma: {ClassCode}", classCode);
            throw new InvalidOperationException($"Erro ao acessar banco de dados: {ex.Message}", ex);
        }
    }

    public async Task<int> GetTotalEnrollmentsAsync(Guid classId)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            return await context.Enrollments
                .AsNoTracking()
                .Where(e => e.IdClass == classId)
                .CountAsync()
                .ConfigureAwait(false);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Erro de SQL ao contar matrículas da turma: {ClassId}", classId);
            throw new InvalidOperationException($"Erro ao acessar banco de dados: {ex.Message}", ex);
        }
    }

}

