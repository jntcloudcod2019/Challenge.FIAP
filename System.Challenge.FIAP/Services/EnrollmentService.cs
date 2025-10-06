using System.Challenge.FIAP.DTOs;
using System.Challenge.FIAP.Entities;
using System.Challenge.FIAP.Interface;
using System.Challenge.FIAP.Request;
using System.Challenge.FIAP.Response;

namespace System.Challenge.FIAP.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IClassRepository _classRepository;
    private readonly ILogger<EnrollmentService> _logger;

    public EnrollmentService(
        IEnrollmentRepository enrollmentRepository,
        IStudentRepository studentRepository,
        IClassRepository classRepository,
        ILogger<EnrollmentService> logger)
    {
        _enrollmentRepository = enrollmentRepository;
        _studentRepository = studentRepository;
        _classRepository = classRepository;
        _logger = logger;
    }

    public async Task<ApiResponse<EnrollmentDto>> CreateEnrollmentAsync(CreateEnrollmentRequest request)
    {
        try
        {
            var student = await _studentRepository.FindStudentAsync(request.StudentDocumentOrRA);
            if (student == null)
            {
                return ApiResponse<EnrollmentDto>.ErrorResponse(
                    $"Aluno não encontrado com Documento/RA: {request.StudentDocumentOrRA}");
            }

            Guid? classId = null;
            if (!string.IsNullOrWhiteSpace(request.ClassCode))
            {
                var classEntity = await _classRepository.GetClassByCodeAsync(request.ClassCode);
                if (classEntity == null)
                {
                    return ApiResponse<EnrollmentDto>.ErrorResponse(
                        $"Turma não encontrada com código: {request.ClassCode}");
                }
                classId = classEntity.IdClass;
            }

            if (classId.HasValue)
            {
                var existingEnrollments = await _enrollmentRepository.GetEnrollmentsByClassIdAsync(classId.Value);
                if (existingEnrollments.Any(e => e.IdStudent == student.IdStudent))
                {
                    return ApiResponse<EnrollmentDto>.ErrorResponse(
                        "Aluno já está matriculado nesta turma");
                }
            }

            var enrollment = new Enrollment
            {
                IdStudent = student.IdStudent,
                IdClass = classId,
                EnrollmentDate = DateTime.UtcNow,
                Status = "Active",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdEnrollment = await _enrollmentRepository.CreateEnrollmentAsync(enrollment);

            var enrollmentDto = MapToEnrollmentDto(createdEnrollment);

            return ApiResponse<EnrollmentDto>.SuccessResponse(enrollmentDto, 
                $"Matrícula criada com sucesso para {student.FullName}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar matrícula: {StudentDoc}", 
                request.StudentDocumentOrRA);
            return ApiResponse<EnrollmentDto>.ErrorResponse($"Erro interno ao criar matrícula: {ex.Message}");
        }
    }

    public async Task<ApiResponse<EnrollmentDto>> GetEnrollmentByIdAsync(Guid id)
    {
        try
        {
            var enrollments = await _enrollmentRepository.GetAllEnrollmentsAsync();
            var enrollment = enrollments.FirstOrDefault(e => e.IdEnrollment == id);

            if (enrollment == null)
            {
                return ApiResponse<EnrollmentDto>.ErrorResponse("Matrícula não encontrada");
            }

            var enrollmentDto = MapToEnrollmentDto(enrollment);

            return ApiResponse<EnrollmentDto>.SuccessResponse(enrollmentDto, "Matrícula encontrada");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar matrícula por ID: {Id}", id);
            return ApiResponse<EnrollmentDto>.ErrorResponse($"Erro interno ao buscar matrícula: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<EnrollmentDto>>> GetAllEnrollmentsAsync()
    {
        try
        {
            var enrollments = await _enrollmentRepository.GetAllEnrollmentsAsync();

            var enrollmentDtos = enrollments.Select(MapToEnrollmentDto).ToList();

            var message = enrollmentDtos.Count > 0
                ? $"Total de {enrollmentDtos.Count} matrícula(s) encontrada(s)"
                : "Nenhuma matrícula encontrada";

            return ApiResponse<List<EnrollmentDto>>.SuccessResponse(enrollmentDtos, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar todas as matrículas");
            return ApiResponse<List<EnrollmentDto>>.ErrorResponse($"Erro interno ao buscar matrículas: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<EnrollmentDto>>> GetEnrollmentsByStudentIdAsync(Guid studentId)
    {
        try
        {
            var enrollments = await _enrollmentRepository.GetEnrollmentsByStudentIdAsync(studentId);

            var enrollmentDtos = enrollments.Select(MapToEnrollmentDto).ToList();

            var message = enrollmentDtos.Count > 0
                ? $"Total de {enrollmentDtos.Count} matrícula(s) encontrada(s) para o aluno"
                : "Nenhuma matrícula encontrada para o aluno";

            return ApiResponse<List<EnrollmentDto>>.SuccessResponse(enrollmentDtos, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar matrículas por ID do aluno: {StudentId}", studentId);
            return ApiResponse<List<EnrollmentDto>>.ErrorResponse($"Erro interno ao buscar matrículas: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<EnrollmentDto>>> GetEnrollmentsByClassIdAsync(Guid classId)
    {
        try
        {
            var enrollments = await _enrollmentRepository.GetEnrollmentsByClassIdAsync(classId);

            var enrollmentDtos = enrollments.Select(MapToEnrollmentDto).ToList();

            var message = enrollmentDtos.Count > 0
                ? $"Total de {enrollmentDtos.Count} matrícula(s) encontrada(s) para a turma"
                : "Nenhuma matrícula encontrada para a turma";

            return ApiResponse<List<EnrollmentDto>>.SuccessResponse(enrollmentDtos, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar matrículas por ID da turma: {ClassId}", classId);
            return ApiResponse<List<EnrollmentDto>>.ErrorResponse($"Erro interno ao buscar matrículas: {ex.Message}");
        }
    }

    public async Task<ApiResponse<List<EnrollmentDto>>> SearchEnrollmentsAsync(Guid? studentId, Guid? classId, Guid? enrollmentId, string? status)
    {
        try
        {
            if (!studentId.HasValue && !classId.HasValue && !enrollmentId.HasValue && string.IsNullOrWhiteSpace(status))
            {
                return ApiResponse<List<EnrollmentDto>>.ErrorResponse("Informe ao menos um parâmetro de busca");
            }

            var enrollments = await _enrollmentRepository.SearchEnrollmentsAsync(studentId, null, classId, status);

            var enrollmentDtos = enrollments.Select(MapToEnrollmentDto).ToList();

            var message = enrollmentDtos.Count > 0
                ? $"Total de {enrollmentDtos.Count} matrícula(s) encontrada(s)"
                : "Nenhuma matrícula encontrada com os critérios informados";

            return ApiResponse<List<EnrollmentDto>>.SuccessResponse(enrollmentDtos, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar matrículas com filtros");
            return ApiResponse<List<EnrollmentDto>>.ErrorResponse($"Erro interno ao buscar matrículas: {ex.Message}");
        }
    }

    public async Task<ApiResponse<EnrollmentDto>> UpdateEnrollmentAsync(Guid id, UpdateEnrollmentRequest request)
    {
        try
        {
            var existingEnrollment = await _enrollmentRepository.GetEnrollmentByIdAsync(id);

            if (existingEnrollment == null)
            {
                return ApiResponse<EnrollmentDto>.ErrorResponse("Matrícula não encontrada");
            }

            var updatedEnrollment = existingEnrollment with
            {
                Status = request.Status ?? existingEnrollment.Status,
                UpdatedAt = DateTime.UtcNow
            };

            await _enrollmentRepository.UpdateEnrollmentAsync(updatedEnrollment);

            var enrollmentWithDetails = await _enrollmentRepository.GetEnrollmentByIdAsync(id);

            var enrollmentDto = MapToEnrollmentDto(enrollmentWithDetails!);

            return ApiResponse<EnrollmentDto>.SuccessResponse(enrollmentDto, "Matrícula atualizada com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar matrícula: {Id}", id);
            return ApiResponse<EnrollmentDto>.ErrorResponse($"Erro interno ao atualizar matrícula: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> DeleteEnrollmentAsync(Guid id)
    {
        try
        {
            var exists = await _enrollmentRepository.EnrollmentExistsAsync(id);

            if (!exists)
            {
                return ApiResponse<bool>.ErrorResponse("Matrícula não encontrada");
            }

            var deleted = await _enrollmentRepository.DeleteEnrollmentAsync(id);

            if (!deleted)
            {
                return ApiResponse<bool>.ErrorResponse("Erro ao deletar matrícula");
            }

            return ApiResponse<bool>.SuccessResponse(true, "Matrícula deletada com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar matrícula: {Id}", id);
            return ApiResponse<bool>.ErrorResponse($"Erro interno ao deletar matrícula: {ex.Message}");
        }
    }

    private static EnrollmentDto MapToEnrollmentDto(Enrollment enrollment)
    {
        return new EnrollmentDto
        {
            IdEnrollment = enrollment.IdEnrollment,
            IdStudent = enrollment.IdStudent,
            IdClass = enrollment.IdClass,
            EnrollmentDate = enrollment.EnrollmentDate,
            CreatedAt = enrollment.CreatedAt,
            UpdatedAt = enrollment.UpdatedAt,
            Status = enrollment.Status,
            StudentName = enrollment.Student?.FullName,
            ClassName = enrollment.Class?.ClassCode
        };
    }
}

