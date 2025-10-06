using Microsoft.EntityFrameworkCore;
using System.Challenge.FIAP.Data;

namespace System.Challenge.FIAP.Services;

public interface IClassCodeService
{
    Task<string> GenerateNextClassCodeAsync();
}

public class ClassCodeService : IClassCodeService
{
    private readonly IDbContextFactory<ContextDB> _contextFactory;
    private readonly ILogger<ClassCodeService> _logger;

    public ClassCodeService(IDbContextFactory<ContextDB> contextFactory, ILogger<ClassCodeService> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<string> GenerateNextClassCodeAsync()
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var lastClass = await context.Classes
                .AsNoTracking()
                .Where(c => c.ClassCode.StartsWith("CLS"))
                .OrderByDescending(c => c.ClassCode)
                .FirstOrDefaultAsync();

            if (lastClass == null)
            {
                return "CLS01";
            }

            var lastCode = lastClass.ClassCode;
            if (lastCode.Length >= 5 && int.TryParse(lastCode.Substring(3), out int lastNumber))
            {
                var nextNumber = lastNumber + 1;
                return $"CLS{nextNumber:D2}";
            }

            return "CLS01";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar próximo código de turma");
            throw new InvalidOperationException("Erro ao gerar código da turma", ex);
        }
    }
}
