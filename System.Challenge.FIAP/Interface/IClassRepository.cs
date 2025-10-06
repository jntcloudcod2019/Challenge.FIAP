using System.Challenge.FIAP.Entities;

namespace System.Challenge.FIAP.Interface;

public interface IClassRepository
{
    Task<Class> CreateClassAsync(Class classEntity);
    Task<Class?> GetClassByIdAsync(Guid id);
    Task<Class?> GetClassByCodeAsync(string classCode);
    Task<List<Class>> GetAllClassesAsync();
    Task<List<Class>> SearchClassesAsync(string query);
    Task<Class> UpdateClassAsync(Class classEntity);
    Task<bool> DeleteClassAsync(Guid id);
    Task<bool> ClassCodeExistsAsync(string classCode);
    Task<int> GetTotalEnrollmentsAsync(Guid classId);
}