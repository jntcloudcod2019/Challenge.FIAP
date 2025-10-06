using System.Challenge.FIAP.Entities;

namespace System.Challenge.FIAP.Interface;

public interface IStudentRepository
{
    Task<Student> CreateStudentAsync(Student student);
    Task<Student?> GetStudentByIdAsync(Guid id);
    Task<Student?> GetStudentByUserIdAsync(Guid userId);
    Task<Student?> GetStudentByRegistrationNumberAsync(string registrationNumber);
    Task<Student?> GetStudentByCpfAsync(string cpf);
    Task<List<Student>> GetAllStudentsAsync();
    Task<List<Student>> GetAllStudentsPaginatedAsync(int pageNumber, int pageSize);
    Task<int> GetTotalStudentsCountAsync();
    Task<List<Student>> SearchStudentsAsync(string query);
    Task<List<Student>> SearchStudentsPaginatedAsync(string query, int pageNumber, int pageSize);
    Task<int> GetSearchStudentsCountAsync(string query);
    Task<Student> UpdateStudentAsync(Student student);
    Task<bool> DeleteStudentAsync(Guid id);
    Task<Student?> FindStudentAsync(string searchTerm);
    Task<bool> RegistrationNumberExistsAsync(string registrationNumber);
    Task<bool> CpfExistsAsync(string cpf);
    Task<bool> UserAlreadyHasStudentProfileAsync(Guid userId);
    Task<int> GetActiveEnrollmentsCountAsync(Guid studentId);
    Task<int> GetTotalEnrollmentsCountAsync();
}

