using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Challenge.FIAP.Entities;
using System.Challenge.FIAP.Interface;

namespace System.Challenge.FIAP.Data;

public class StudentRepository : IStudentRepository
{
    private readonly IDbContextFactory<ContextDB> _contextFactory;
    private readonly ILogger<StudentRepository> _logger;

    public StudentRepository(IDbContextFactory<ContextDB> contextFactory, ILogger<StudentRepository> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<Student> CreateStudentAsync(Student student)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            context.Students.Add(student);
            await context.SaveChangesAsync().ConfigureAwait(false);
            
            return student;
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Erro de SQL ao criar aluno: {RegistrationNumber}", student.RegistrationNumber);
            throw new InvalidOperationException($"Erro ao acessar banco de dados: {ex.Message}", ex);
        }
       
    }

    public async Task<Student?> GetStudentByIdAsync(Guid id)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            return await context.Students
                .AsNoTracking()
                .Include(s => s.User)
                .Include(s => s.Enrollments)
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Class)
                .Where(s => s.IdStudent == id)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Erro de SQL ao buscar aluno por ID: {Id}", id);
            throw new InvalidOperationException($"Erro ao acessar banco de dados: {ex.Message}", ex);
        }
       
    }

    public async Task<Student?> GetStudentByCpfAsync(string cpf)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            return await context.Students
                .AsNoTracking()
                .Include(s => s.User)
                .Include(s => s.Enrollments)
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Class)
                .Where(s => s.Cpf == cpf)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Erro de SQL ao buscar aluno por CPF: {Cpf}", cpf);
            throw new InvalidOperationException($"Erro ao acessar banco de dados: {ex.Message}", ex);
        }
       
    }

    public async Task<Student?> GetStudentByUserIdAsync(Guid userId)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            return await context.Students
                .AsNoTracking()
                .Include(s => s.User)
                .Include(s => s.Enrollments)
                .Where(s => s.IdUser == userId)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Erro de SQL ao buscar aluno por IdUser: {IdUser}", userId);
            throw new InvalidOperationException($"Erro ao acessar banco de dados: {ex.Message}", ex);
        }
     
    }

    public async Task<Student?> GetStudentByRegistrationNumberAsync(string registrationNumber)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            return await context.Students
                .AsNoTracking()
                .Include(s => s.User)
                .Where(s => s.RegistrationNumber == registrationNumber)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Erro de SQL ao buscar aluno por RA: {RA}", registrationNumber);
            throw new InvalidOperationException($"Erro ao acessar banco de dados: {ex.Message}", ex);
        }
       
    }

    public async Task<List<Student>> GetAllStudentsAsync()
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            return await context.Students
                .AsNoTracking()
                .Include(s => s.User)
                .Include(s => s.Enrollments)
                .OrderBy(s => s.User.FullName)
                .ToListAsync()
                .ConfigureAwait(false);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Erro de SQL ao buscar todos os alunos");
            throw new InvalidOperationException($"Erro ao acessar banco de dados: {ex.Message}", ex);
        }
        
    }

    public async Task<List<Student>> GetAllStudentsPaginatedAsync(int pageNumber, int pageSize)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            var skip = (pageNumber - 1) * pageSize;
            
            return await context.Students
                .AsNoTracking()
                .Include(s => s.User)
                .Include(s => s.Enrollments)
                .OrderBy(s => s.User.FullName)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync()
                .ConfigureAwait(false);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Erro de SQL ao buscar alunos paginados");
            throw new InvalidOperationException($"Erro ao acessar banco de dados: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar alunos paginados");
            throw new InvalidOperationException($"Erro interno: {ex.Message}", ex);
        }
    }

    public async Task<int> GetTotalStudentsCountAsync()
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            return await context.Students
                .AsNoTracking()
                .CountAsync()
                .ConfigureAwait(false);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Erro de SQL ao contar alunos");
            throw new InvalidOperationException($"Erro ao acessar banco de dados: {ex.Message}", ex);
        }
      
    }

    public async Task<List<Student>> SearchStudentsAsync(string query)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            return await context.Students
                .AsNoTracking()
                .Include(s => s.User)
                .Include(s => s.Enrollments)
                .Where(s => 
                    s.RegistrationNumber.Contains(query) ||
                    s.Cpf.Contains(query) ||
                    s.FullName.Contains(query) ||
                    s.User.Document.Contains(query) ||
                    s.User.Email.Contains(query) ||
                    s.User.FullName.Contains(query))
                .OrderBy(s => s.User.FullName)
                .ToListAsync()
                .ConfigureAwait(false);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Erro de SQL ao buscar alunos");
            throw new InvalidOperationException($"Erro ao acessar banco de dados: {ex.Message}", ex);
        }
       
    }

    public async Task<List<Student>> SearchStudentsPaginatedAsync(string query, int pageNumber, int pageSize)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            var skip = (pageNumber - 1) * pageSize;
            
            return await context.Students
                .AsNoTracking()
                .Include(s => s.User)
                .Include(s => s.Enrollments)
                .Where(s => 
                    s.RegistrationNumber.Contains(query) ||
                    s.Cpf.Contains(query) ||
                    s.FullName.Contains(query) ||
                    s.User.Document.Contains(query) ||
                    s.User.Email.Contains(query) ||
                    s.User.FullName.Contains(query))
                .OrderBy(s => s.User.FullName)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync()
                .ConfigureAwait(false);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Erro de SQL ao buscar alunos paginados");
            throw new InvalidOperationException($"Erro ao acessar banco de dados: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar alunos paginados");
            throw new InvalidOperationException($"Erro interno: {ex.Message}", ex);
        }
    }

    public async Task<int> GetSearchStudentsCountAsync(string query)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            return await context.Students
                .AsNoTracking()
                .Where(s => 
                    s.RegistrationNumber.Contains(query) ||
                    s.Cpf.Contains(query) ||
                    s.FullName.Contains(query) ||
                    s.User.Document.Contains(query) ||
                    s.User.Email.Contains(query) ||
                    s.User.FullName.Contains(query))
                .CountAsync()
                .ConfigureAwait(false);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Erro de SQL ao contar alunos na busca");
            throw new InvalidOperationException($"Erro ao acessar banco de dados: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao contar alunos na busca");
            throw new InvalidOperationException($"Erro interno: {ex.Message}", ex);
        }
    }

    public async Task<Student> UpdateStudentAsync(Student student)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            context.Students.Update(student);
            await context.SaveChangesAsync().ConfigureAwait(false);
            
            return student;
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Erro de SQL ao atualizar aluno: {Id}", student.IdStudent);
            throw new InvalidOperationException($"Erro ao acessar banco de dados: {ex.Message}", ex);
        }
       
    }

    public async Task<bool> DeleteStudentAsync(Guid id)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            var student = await context.Students.FindAsync(id);
            
            if (student == null)
            {
                return false;
            }

            context.Students.Remove(student);
            await context.SaveChangesAsync().ConfigureAwait(false);
            
            return true;
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Erro de SQL ao deletar aluno: {Id}", id);
            throw new InvalidOperationException($"Erro ao acessar banco de dados: {ex.Message}", ex);
        }
       
    }

    public async Task<Student?> FindStudentAsync(string searchTerm)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            return await context.Students
                .AsNoTracking()
                .Where(s => 
                    s.RegistrationNumber == searchTerm ||
                    s.Cpf == searchTerm ||
                    s.User.Document == searchTerm ||
                    s.User.Email == searchTerm ||
                    s.User.FullName.Contains(searchTerm) ||
                    s.FullName.Contains(searchTerm))
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Erro de SQL ao buscar aluno por termo: {SearchTerm}", searchTerm);
            throw new InvalidOperationException($"Erro ao acessar banco de dados: {ex.Message}", ex);
        }
        
    }

    public async Task<bool> RegistrationNumberExistsAsync(string registrationNumber)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            return await context.Students
                .AsNoTracking()
                .AnyAsync(s => s.RegistrationNumber == registrationNumber)
                .ConfigureAwait(false);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Erro de SQL ao verificar existência de RA: {RA}", registrationNumber);
            throw new InvalidOperationException($"Erro ao acessar banco de dados: {ex.Message}", ex);
        }
            
    }

    public async Task<bool> CpfExistsAsync(string cpf)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            return await context.Students
                .AsNoTracking()
                .AnyAsync(s => s.Cpf == cpf)
                .ConfigureAwait(false);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Erro de SQL ao verificar existência de CPF: {Cpf}", cpf);
            throw new InvalidOperationException($"Erro ao acessar banco de dados: {ex.Message}", ex);
        }
            
    }

    public async Task<bool> UserAlreadyHasStudentProfileAsync(Guid userId)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            return await context.Students
                .AsNoTracking()
                .AnyAsync(s => s.IdUser == userId)
                .ConfigureAwait(false);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Erro de SQL ao verificar perfil de aluno para usuário: {IdUser}", userId);
            throw new InvalidOperationException($"Erro ao acessar banco de dados: {ex.Message}", ex);
        }
    }

    public async Task<int> GetActiveEnrollmentsCountAsync(Guid studentId)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            return await context.Enrollments
                .AsNoTracking()
                .Where(e => e.IdStudent == studentId && e.Status == "Active")
                .CountAsync()
                .ConfigureAwait(false);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Erro de SQL ao contar matrículas ativas do aluno: {IdStudent}", studentId);
            throw new InvalidOperationException($"Erro ao acessar banco de dados: {ex.Message}", ex);
        }
    }

    public async Task<int> GetTotalEnrollmentsCountAsync()
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            return await context.Enrollments
                .AsNoTracking()
                .CountAsync()
                .ConfigureAwait(false);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Erro de SQL ao contar total de matrículas do sistema");
            throw new InvalidOperationException($"Erro ao acessar banco de dados: {ex.Message}", ex);
        }
       
    }
}

