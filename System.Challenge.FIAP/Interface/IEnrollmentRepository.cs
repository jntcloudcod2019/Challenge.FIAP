using System.Challenge.FIAP.Entities;

namespace System.Challenge.FIAP.Interface;

public interface IEnrollmentRepository
{
    Task<Enrollment> CreateEnrollmentAsync(Enrollment enrollment);
    Task<Enrollment?> GetEnrollmentByIdAsync(Guid id);
    Task<List<Enrollment>> GetAllEnrollmentsAsync();
    Task<List<Enrollment>> GetEnrollmentsByStudentIdAsync(Guid studentId);
    Task<List<Enrollment>> GetEnrollmentsByClassIdAsync(Guid classId);
    Task<List<Enrollment>> SearchEnrollmentsAsync(Guid? studentId, Guid? courseId, Guid? classId, string? status);
    Task<Enrollment> UpdateEnrollmentAsync(Enrollment enrollment);
    Task<bool> DeleteEnrollmentAsync(Guid id);
    Task<bool> EnrollmentExistsAsync(Guid id);
}
