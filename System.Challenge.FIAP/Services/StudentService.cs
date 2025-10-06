using System.Challenge.FIAP.DTOs;
using System.Challenge.FIAP.Entities;
using System.Challenge.FIAP.Interface;
using System.Challenge.FIAP.Request;
using System.Challenge.FIAP.Response;

namespace System.Challenge.FIAP.Services;

public class StudentService : IStudentService
{
    private readonly IStudentRepository _studentRepository;
    private readonly IUserService _userService;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<StudentService> _logger;

    public StudentService(
        IStudentRepository studentRepository,
        IUserService userService,
        IUserRepository userRepository,
        ILogger<StudentService> logger)
    {
        _studentRepository = studentRepository;
        _userService = userService;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<ApiResponse<StudentDto>> CreateStudentAsync(CreateStudentRequest request)
    {
        try
        {
            var cpfExists = await _studentRepository.CpfExistsAsync(request.Cpf);
            if (cpfExists)
            {
                return ApiResponse<StudentDto>.ErrorResponse("CPF já cadastrado");
            }

            var raExists = await _studentRepository.RegistrationNumberExistsAsync(request.RegistrationNumber);
            if (raExists)
            {
                return ApiResponse<StudentDto>.ErrorResponse("RA (Registro Acadêmico) já cadastrado");
            }

            var userRequest = new CreateUserRequest
            {
                FullName = request.FullName,
                Email = request.Email,
                Document = request.Cpf,
                Role = "Student",
                StatusAccount = true
            };

            var (createdUser, generatedPassword) = await _userService.CreateStudentUserAsync(userRequest);

            var student = new Student
            {
                IdUser = createdUser.IdUser,
                RegistrationNumber = request.RegistrationNumber,
                FullName = request.FullName,
                Cpf = request.Cpf,
                BirthDate = request.BirthDate,
                Address = request.Address,
                PhoneNumber = request.PhoneNumber,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdStudent = await _studentRepository.CreateStudentAsync(student);

            var studentWithDetails = await _studentRepository.GetStudentByIdAsync(createdStudent.IdStudent);

            var studentDto = await MapToStudentDtoAsync(studentWithDetails!);

            return ApiResponse<StudentDto>.SuccessResponse(studentDto, $"Aluno criado com sucesso. Senha gerada: {generatedPassword}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar aluno: {RegistrationNumber}", request.RegistrationNumber);
            return ApiResponse<StudentDto>.ErrorResponse($"Erro interno ao criar aluno: {ex.Message}");
        }
    }

    public async Task<ApiResponse<StudentDto>> GetStudentByIdAsync(Guid id)
    {
        try
        {
            var student = await _studentRepository.GetStudentByIdAsync(id);

            if (student == null)
            {
                return ApiResponse<StudentDto>.ErrorResponse("Aluno não encontrado");
            }

            var studentDto = await MapToStudentDtoAsync(student);

            return ApiResponse<StudentDto>.SuccessResponse(studentDto, "Aluno encontrado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar aluno por ID: {Id}", id);
            return ApiResponse<StudentDto>.ErrorResponse($"Erro interno ao buscar aluno: {ex.Message}");
        }
    }

    public async Task<ApiResponse<StudentDto>> GetStudentByRegistrationNumberAsync(string registrationNumber)
    {
        try
        {
            var student = await _studentRepository.GetStudentByRegistrationNumberAsync(registrationNumber);

            if (student == null)
            {
                return ApiResponse<StudentDto>.ErrorResponse("Aluno não encontrado");
            }

            var studentDto = await MapToStudentDtoAsync(student);

            return ApiResponse<StudentDto>.SuccessResponse(studentDto, "Aluno encontrado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar aluno por RA: {RA}", registrationNumber);
            return ApiResponse<StudentDto>.ErrorResponse($"Erro interno ao buscar aluno: {ex.Message}");
        }
    }

    public async Task<ApiResponse<StudentDto>> FindStudentAsync(string searchTerm)
    {
        try
        {
            var student = await _studentRepository.FindStudentAsync(searchTerm);

            if (student == null)
            {
                return ApiResponse<StudentDto>.ErrorResponse("Aluno não encontrado com o termo de busca informado");
            }

            var studentDto = await MapToStudentDtoAsync(student);

            return ApiResponse<StudentDto>.SuccessResponse(studentDto, "Aluno encontrado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar aluno por termo: {SearchTerm}", searchTerm);
            return ApiResponse<StudentDto>.ErrorResponse($"Erro interno ao buscar aluno: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<StudentDto>>> GetAllStudentsAsync()
    {
        try
        {
            var students = await _studentRepository.GetAllStudentsAsync();

            var studentDtos = new List<StudentDto>();
            foreach (var student in students)
            {
                studentDtos.Add(await MapToStudentDtoAsync(student));
            }

            var message = studentDtos.Count > 0
                ? $"Total de {studentDtos.Count} aluno(s) encontrado(s)"
                : "Nenhum aluno encontrado";

            return ApiResponse<List<StudentDto>>.SuccessResponse(studentDtos, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar todos os alunos");
            return ApiResponse<List<StudentDto>>.ErrorResponse($"Erro interno ao buscar alunos: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> DeleteStudentByQueryAsync(string query)
    {
        try
        {
            var student = await _studentRepository.FindStudentAsync(query);

            if (student == null)
            {
                return ApiResponse<bool>.ErrorResponse("Aluno não encontrado");
            }

            var activeEnrollments = await _studentRepository.GetActiveEnrollmentsCountAsync(student.IdStudent);
            if (activeEnrollments > 0)
            {
                return ApiResponse<bool>.ErrorResponse($"Não é possível deletar aluno com {activeEnrollments} matrícula(s) ativa(s)");
            }

            var deleted = await _studentRepository.DeleteStudentAsync(student.IdStudent);

            if (!deleted)
            {
                return ApiResponse<bool>.ErrorResponse("Erro ao deletar aluno");
            }

            return ApiResponse<bool>.SuccessResponse(true, "Aluno deletado com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar aluno por query: {Query}", query);
            return ApiResponse<bool>.ErrorResponse($"Erro interno ao deletar aluno: {ex.Message}");
        }
    }

    private async Task<StudentDto> MapToStudentDtoAsync(Student student)
    {
        var user = await _userRepository.GetUserByIdAsync(student.IdUser);
        
        return new StudentDto
        {
            IdStudent = student.IdStudent,
            IdUser = student.IdUser,
            RegistrationNumber = student.RegistrationNumber,
            Cpf = student.Cpf,
            FullName = student.FullName,
            CreatedAt = student.CreatedAt,
            UpdatedAt = student.UpdatedAt,
            UserFullName = user?.FullName,
            UserEmail = user?.Email,
            UserDocument = user?.Document,
            UserStatus = user?.StatusAccount,
            TotalEnrollments = student.Enrollments?.Count ?? 0,
            ActiveEnrollments = student.Enrollments?.Count(e => e.Status == "Active") ?? 0
        };
    }

    public async Task<ApiResponse<StudentDto>> UpdateStudentAsync(Guid id, UpdateStudentRequest request)
    {
        try
        {
            var existingStudent = await _studentRepository.GetStudentByIdAsync(id);

            if (existingStudent == null)
            {
                return ApiResponse<StudentDto>.ErrorResponse("Estudante não encontrado");
            }

            var updatedStudent = existingStudent with
            {
                FullName = request.FullName ?? existingStudent.FullName,
                BirthDate = request.BirthDate ?? existingStudent.BirthDate,
                Address = request.Address ?? existingStudent.Address,
                PhoneNumber = request.PhoneNumber ?? existingStudent.PhoneNumber,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await _studentRepository.UpdateStudentAsync(updatedStudent);
            var studentDto = await MapToStudentDtoAsync(result);

            _logger.LogInformation("Estudante atualizado: {FullName} (ID: {Id})", result.FullName, id);

            return ApiResponse<StudentDto>.SuccessResponse(studentDto, "Estudante atualizado com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar estudante: {Id}", id);
            return ApiResponse<StudentDto>.ErrorResponse("Erro ao atualizar estudante", new List<string> { ex.Message });
        }
    }

}

