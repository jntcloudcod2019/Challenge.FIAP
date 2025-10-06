using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Challenge.FIAP.Entities;
using System.Challenge.FIAP.Interface;

namespace System.Challenge.FIAP.Data;

public class EnrollmentRepository : IEnrollmentRepository
{
    private readonly IDbContextFactory<ContextDB> _contextFactory;
    private readonly ILogger<EnrollmentRepository> _logger;

    public EnrollmentRepository(IDbContextFactory<ContextDB> contextFactory, ILogger<EnrollmentRepository> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<Enrollment> CreateEnrollmentAsync(Enrollment enrollment)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            context.Enrollments.Add(enrollment);
            await context.SaveChangesAsync().ConfigureAwait(false);
            
            return enrollment;
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Erro de SQL ao criar matrícula para aluno: {IdStudent}", enrollment.IdStudent);
            throw new InvalidOperationException($"Erro ao acessar banco de dados: {ex.Message}", ex);
        }
       
    }

    public async Task<List<Enrollment>> GetAllEnrollmentsAsync()
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            return await context.Enrollments
                .AsNoTracking()
                .ToListAsync()
                .ConfigureAwait(false);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Erro de SQL ao buscar todas as matrículas");
            throw new InvalidOperationException($"Erro ao acessar banco de dados: {ex.Message}", ex);
        }
       
    }

    public async Task<List<Enrollment>> SearchEnrollmentsAsync(Guid? studentId, Guid? courseId, Guid? classId, string? status)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            var query = context.Enrollments
                .AsNoTracking()
                .Include(e => e.Student)
                    .ThenInclude(s => s.User)
                .Include(e => e.Class)
                .AsQueryable();

            if (studentId.HasValue)
            {
                query = query.Where(e => e.IdStudent == studentId.Value);
            }

            if (classId.HasValue)
            {
                query = query.Where(e => e.IdClass == classId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(e => e.Status == status);
            }

            return await query
                .OrderBy(e => e.Student.User.FullName)
                .ToListAsync()
                .ConfigureAwait(false);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Erro de SQL ao buscar matrículas com filtros");
            throw new InvalidOperationException($"Erro ao acessar banco de dados: {ex.Message}", ex);
        }
       
    }

    public async Task<Enrollment> UpdateEnrollmentAsync(Enrollment enrollment)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            context.Enrollments.Update(enrollment);
            await context.SaveChangesAsync().ConfigureAwait(false);
            
            return enrollment;
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Erro de SQL ao atualizar matrícula: {Id}", enrollment.IdEnrollment);
            throw new InvalidOperationException($"Erro ao acessar banco de dados: {ex.Message}", ex);
        }
    
    }

    public async Task<bool> DeleteEnrollmentAsync(Guid id)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            var enrollment = await context.Enrollments.FindAsync(id);
            
            if (enrollment == null)
            {
                return false;
            }

            context.Enrollments.Remove(enrollment);
            await context.SaveChangesAsync().ConfigureAwait(false);
            
            return true;
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Erro de SQL ao deletar matrícula: {Id}", id);
            throw new InvalidOperationException($"Erro ao acessar banco de dados: {ex.Message}", ex);
        }
      
    }

    public async Task<Enrollment?> GetEnrollmentByIdAsync(Guid id)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            return await context.Enrollments
                .AsNoTracking()
                .Include(e => e.Student)
                .Include(e => e.Class)
                .FirstOrDefaultAsync(e => e.IdEnrollment == id)
                .ConfigureAwait(false);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Erro de SQL ao buscar matrícula por ID: {Id}", id);
            throw new InvalidOperationException($"Erro ao acessar banco de dados: {ex.Message}", ex);
        }
    }

    public async Task<List<Enrollment>> GetEnrollmentsByStudentIdAsync(Guid studentId)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            return await context.Enrollments
                .AsNoTracking()
                .Include(e => e.Student)
                .Include(e => e.Class)
                .Where(e => e.IdStudent == studentId)
                .OrderBy(e => e.EnrollmentDate)
                .ToListAsync()
                .ConfigureAwait(false);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Erro de SQL ao buscar matrículas por ID do aluno: {StudentId}", studentId);
            throw new InvalidOperationException($"Erro ao acessar banco de dados: {ex.Message}", ex);
        }
    }

    public async Task<List<Enrollment>> GetEnrollmentsByClassIdAsync(Guid classId)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            return await context.Enrollments
                .AsNoTracking()
                .Include(e => e.Student)
                .Include(e => e.Class)
                .Where(e => e.IdClass == classId)
                .OrderBy(e => e.EnrollmentDate)
                .ToListAsync()
                .ConfigureAwait(false);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Erro de SQL ao buscar matrículas por ID da turma: {ClassId}", classId);
            throw new InvalidOperationException($"Erro ao acessar banco de dados: {ex.Message}", ex);
        }
    }

    public async Task<bool> EnrollmentExistsAsync(Guid id)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            
            return await context.Enrollments
                .AsNoTracking()
                .AnyAsync(e => e.IdEnrollment == id)
                .ConfigureAwait(false);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Erro de SQL ao verificar existência da matrícula: {Id}", id);
            throw new InvalidOperationException($"Erro ao acessar banco de dados: {ex.Message}", ex);
        }
    }

}

