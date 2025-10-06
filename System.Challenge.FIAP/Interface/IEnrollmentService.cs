using System.Challenge.FIAP.DTOs;
using System.Challenge.FIAP.Request;
using System.Challenge.FIAP.Response;

namespace System.Challenge.FIAP.Interface;

public interface IEnrollmentService
{
    Task<ApiResponse<EnrollmentDto>> CreateEnrollmentAsync(CreateEnrollmentRequest request);
    Task<ApiResponse<EnrollmentDto>> GetEnrollmentByIdAsync(Guid id);
    Task<ApiResponse<List<EnrollmentDto>>> GetAllEnrollmentsAsync();
    Task<ApiResponse<List<EnrollmentDto>>> GetEnrollmentsByStudentIdAsync(Guid studentId);
    Task<ApiResponse<List<EnrollmentDto>>> GetEnrollmentsByClassIdAsync(Guid classId);
    Task<ApiResponse<List<EnrollmentDto>>> SearchEnrollmentsAsync(Guid? studentId, Guid? classId, Guid? enrollmentId, string? status);
    Task<ApiResponse<EnrollmentDto>> UpdateEnrollmentAsync(Guid id, UpdateEnrollmentRequest request);
    Task<ApiResponse<bool>> DeleteEnrollmentAsync(Guid id);
}