using System.Challenge.FIAP.DTOs;
using System.Challenge.FIAP.Entities;
using System.Challenge.FIAP.Interface;
using System.Challenge.FIAP.Request;
using System.Challenge.FIAP.Response;

namespace System.Challenge.FIAP.Services;

public class ClassService : IClassService
{
    private readonly IClassRepository _classRepository;
    private readonly IClassCodeService _classCodeService;
    private readonly ILogger<ClassService> _logger;

    public ClassService(
        IClassRepository classRepository,
        IClassCodeService classCodeService,
        ILogger<ClassService> logger)
    {
        _classRepository = classRepository;
        _classCodeService = classCodeService;
        _logger = logger;
    }

    public async Task<ApiResponse<ClassDto>> CreateClassAsync(CreateClassRequest request)
    {
        try
        {
            var classCode = await _classCodeService.GenerateNextClassCodeAsync();

            Guid? professorId = null;

            var classEntity = new Class
            {
                IdProfessor = professorId,
                ClassCode = classCode,
                Name = request.Name,
                Description = request.Description,
                Capacity = 50,
                Room = request.Room,
                Status = "Open",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdClass = await _classRepository.CreateClassAsync(classEntity);

            var classWithDetails = await _classRepository.GetClassByIdAsync(createdClass.IdClass);
            var classDto = await MapToClassDtoAsync(classWithDetails!);

            return ApiResponse<ClassDto>.SuccessResponse(classDto, 
                $"Turma {classCode} criada com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar turma: {ClassName}", request.Name);
            return ApiResponse<ClassDto>.ErrorResponse($"Erro interno ao criar turma: {ex.Message}");
        }
    }

    public async Task<ClassDto?> GetClassByIdAsync(Guid id)
    {
        var classEntity = await _classRepository.GetClassByIdAsync(id);

        if (classEntity == null)
        {
            return null;
        }

        return await MapToClassDtoAsync(classEntity);
    }

    public async Task<ClassDto?> GetClassByCodeAsync(string classCode)
    {
        var classEntity = await _classRepository.GetClassByCodeAsync(classCode);

        if (classEntity == null)
        {
            return null;
        }

        return await MapToClassDtoAsync(classEntity);
    }

    public async Task<List<ClassDto>> GetAllClassesAsync()
    {
        var classes = await _classRepository.GetAllClassesAsync();
        var classDtos = new List<ClassDto>();

        foreach (var classEntity in classes)
        {
            var dto = await MapToClassDtoAsync(classEntity);
            classDtos.Add(dto);
        }

        return classDtos;
    }

    public async Task<List<ClassDto>> GetClassesBySubjectCodeAsync(string subjectCode)
    {
        return new List<ClassDto>();
    }

    public async Task<List<ClassDto>> SearchClassesAsync(string query)
    {
        var classes = await _classRepository.SearchClassesAsync(query);
        var classDtos = new List<ClassDto>();

        foreach (var classEntity in classes)
        {
            var dto = await MapToClassDtoAsync(classEntity);
            classDtos.Add(dto);
        }

        return classDtos;
    }

    public async Task<ClassDto?> UpdateClassAsync(string classCode, UpdateClassRequest request)
    {
        var existingClass = await _classRepository.GetClassByCodeAsync(classCode);

        if (existingClass == null)
        {
            return null;
        }

        Guid? professorId = existingClass.IdProfessor;

        var updatedClass = existingClass with
        {
            IdProfessor = professorId,
            Room = request.Room ?? existingClass.Room,
            Status = request.Status ?? existingClass.Status,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await _classRepository.UpdateClassAsync(updatedClass);
        return await MapToClassDtoAsync(result);
    }

    public async Task<bool> DeleteClassAsync(string classCode)
    {
        var classEntity = await _classRepository.GetClassByCodeAsync(classCode);

        if (classEntity == null)
        {
            return false;
        }

        var totalEnrollments = await _classRepository.GetTotalEnrollmentsAsync(classEntity.IdClass);
        if (totalEnrollments > 0)
        {
            throw new InvalidOperationException(
                $"Não é possível excluir a turma. Existem {totalEnrollments} matrícula(s) vinculada(s)");
        }

        return await _classRepository.DeleteClassAsync(classEntity.IdClass);
    }

    private async Task<ClassDto> MapToClassDtoAsync(Class classEntity)
    {
        var totalEnrollments = await _classRepository.GetTotalEnrollmentsAsync(classEntity.IdClass);
        var availableSeats = classEntity.Capacity - totalEnrollments;

        return new ClassDto
        {
            IdClass = classEntity.IdClass,
            IdProfessor = classEntity.IdProfessor,
            ClassCode = classEntity.ClassCode,
            Name = classEntity.Name,
            Description = classEntity.Description,
            Capacity = classEntity.Capacity,
            CreatedAt = classEntity.CreatedAt,
            UpdatedAt = classEntity.UpdatedAt,
            Room = classEntity.Room,
            Status = classEntity.Status,
            ProfessorName = null,
            ProfessorEmail = null,
            TotalEnrollments = totalEnrollments,
            AvailableSeats = availableSeats
        };
    }
}

