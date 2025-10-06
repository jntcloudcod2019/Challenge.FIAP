using System.Challenge.FIAP.DTOs;
using System.Challenge.FIAP.Request;
using System.Challenge.FIAP.Response;

namespace System.Challenge.FIAP.Interface;

public interface IStudentService
{
    Task<ApiResponse<StudentDto>> CreateStudentAsync(CreateStudentRequest request);
    Task<ApiResponse<StudentDto>> GetStudentByIdAsync(Guid id);
    Task<ApiResponse<StudentDto>> GetStudentByRegistrationNumberAsync(string registrationNumber);
    Task<ApiResponse<StudentDto>> FindStudentAsync(string searchTerm);
    Task<ApiResponse<List<StudentDto>>> GetAllStudentsAsync();
    Task<ApiResponse<StudentDto>> UpdateStudentAsync(Guid id, UpdateStudentRequest request);
    Task<ApiResponse<bool>> DeleteStudentByQueryAsync(string query);
}