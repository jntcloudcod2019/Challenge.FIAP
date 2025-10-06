using System.Challenge.FIAP.DTOs;
using System.Challenge.FIAP.Request;
using System.Challenge.FIAP.Response;

namespace System.Challenge.FIAP.Interface;

public interface IClassService
{
    Task<ApiResponse<ClassDto>> CreateClassAsync(CreateClassRequest request);
    Task<ClassDto?> GetClassByIdAsync(Guid id);
    Task<ClassDto?> GetClassByCodeAsync(string classCode);
    Task<List<ClassDto>> GetAllClassesAsync();
    Task<List<ClassDto>> GetClassesBySubjectCodeAsync(string subjectCode);
    Task<List<ClassDto>> SearchClassesAsync(string query);
    Task<ClassDto?> UpdateClassAsync(string classCode, UpdateClassRequest request);
    Task<bool> DeleteClassAsync(string classCode);
}

