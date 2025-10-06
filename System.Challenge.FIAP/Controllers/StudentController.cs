using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Challenge.FIAP.DTOs;
using System.Challenge.FIAP.Interface;
using System.Challenge.FIAP.Request;
using System.Challenge.FIAP.Response;

namespace System.Challenge.FIAP.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class StudentController : ControllerBase
{
    private readonly IStudentService _studentService;
    private readonly ILogger<StudentController> _logger;

    public StudentController(IStudentService studentService, ILogger<StudentController> logger)
    {
        _studentService = studentService;
        _logger = logger;
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<StudentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<StudentDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateStudent([FromBody] CreateStudentRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<StudentDto>.ErrorResponse("Dados inválidos"));
        }

        var result = await _studentService.CreateStudentAsync(request);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetStudentByRegistrationNumber), new { registrationNumber = result.Data!.RegistrationNumber }, result);
    }

    [HttpGet("ra/{registrationNumber}")]
    [ProducesResponseType(typeof(ApiResponse<StudentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<StudentDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStudentByRegistrationNumber(string registrationNumber)
    {
        var result = await _studentService.GetStudentByRegistrationNumberAsync(registrationNumber);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpGet("find/{searchTerm}")]
    [ProducesResponseType(typeof(ApiResponse<StudentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<StudentDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> FindStudent(string searchTerm)
    {
        var result = await _studentService.FindStudentAsync(searchTerm);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<StudentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PagedResponse<StudentDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAllStudents([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var allStudents = await _studentService.GetAllStudentsAsync();
            var totalRecords = allStudents.Data.Count;
            
            var skip = (pageNumber - 1) * pageSize;
            var data = allStudents.Data.Skip(skip).Take(pageSize).ToList();
            
            var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            var hasPreviousPage = pageNumber > 1;
            var hasNextPage = pageNumber < totalPages;

            return Ok(new { 
                success = true, 
                message = $"{data.Count} aluno(s) encontrado(s) na página {pageNumber}",
                data = data,
                pageNumber = pageNumber,
                pageSize = pageSize,
                totalRecords = totalRecords,
                totalPages = totalPages,
                hasPreviousPage = hasPreviousPage,
                hasNextPage = hasNextPage
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar alunos paginados");
            return BadRequest(new { 
                success = false, 
                message = "Erro ao listar alunos paginados", 
                errors = new List<string> { ex.Message },
                data = new List<StudentDto>(),
                pageNumber = 0,
                pageSize = 0,
                totalRecords = 0,
                totalPages = 0,
                hasPreviousPage = false,
                hasNextPage = false
            });
        }
    }

    [HttpGet("search")]
    [ProducesResponseType(typeof(PagedResponse<StudentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PagedResponse<StudentDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchStudents(
        [FromQuery] string query,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest(new { 
                    success = false, 
                    message = "Informe um termo de busca", 
                    errors = new List<string>(),
                    data = new List<StudentDto>(),
                    pageNumber = 0,
                    pageSize = 0,
                    totalRecords = 0,
                    totalPages = 0,
                    hasPreviousPage = false,
                    hasNextPage = false
                });
            }

            var allStudents = await _studentService.GetAllStudentsAsync();
            var filteredStudents = allStudents.Data.Where(s => 
                s.FullName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                s.RegistrationNumber.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                s.Cpf.Contains(query, StringComparison.OrdinalIgnoreCase)
            ).ToList();
            
            var totalRecords = filteredStudents.Count;
            
            var skip = (pageNumber - 1) * pageSize;
            var data = filteredStudents.Skip(skip).Take(pageSize).ToList();
            
            var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            var hasPreviousPage = pageNumber > 1;
            var hasNextPage = pageNumber < totalPages;

            return Ok(new { 
                success = true, 
                message = $"{data.Count} aluno(s) encontrado(s) na busca, página {pageNumber}",
                data = data,
                pageNumber = pageNumber,
                pageSize = pageSize,
                totalRecords = totalRecords,
                totalPages = totalPages,
                hasPreviousPage = hasPreviousPage,
                hasNextPage = hasNextPage
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar alunos paginados");
            return BadRequest(new { 
                success = false, 
                message = "Erro ao buscar alunos paginados", 
                errors = new List<string> { ex.Message },
                data = new List<StudentDto>(),
                pageNumber = 0,
                pageSize = 0,
                totalRecords = 0,
                totalPages = 0,
                hasPreviousPage = false,
                hasNextPage = false
            });
        }
    }

    [HttpPut("{query}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<StudentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<StudentDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<StudentDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateStudent(string query, [FromBody] UpdateStudentRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<StudentDto>.ErrorResponse("Dados inválidos"));
        }

        var findResult = await _studentService.FindStudentAsync(query);
        if (!findResult.Success)
        {
            return NotFound(new { 
                success = false, 
                message = "Aluno não encontrado", 
                errors = new List<string>(),
                data = (StudentDto?)null
            });
        }
        
        var result = await _studentService.UpdateStudentAsync(findResult.Data!.IdStudent, request);

        if (!result.Success)
        {
            return result.Message.Contains("não encontrado") ? NotFound(result) : BadRequest(result);
        }

        return Ok(result);
    }

    [HttpDelete("{query}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteStudent(string query)
    {
        var result = await _studentService.DeleteStudentByQueryAsync(query);

        if (!result.Success)
        {
            return result.Message.Contains("não encontrado") ? NotFound(result) : BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("statistics")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatistics()
    {
        var allStudents = await _studentService.GetAllStudentsAsync();
        
        var stats = new
        {
            TotalStudents = allStudents.Data?.Count ?? 0,
            TotalEnrollments = allStudents.Data?.Sum(s => s.TotalEnrollments) ?? 0,
            TotalActiveEnrollments = allStudents.Data?.Sum(s => s.ActiveEnrollments) ?? 0,
            StudentsWithActiveEnrollments = allStudents.Data?.Count(s => s.ActiveEnrollments > 0) ?? 0,
            StudentsWithoutEnrollments = allStudents.Data?.Count(s => s.TotalEnrollments == 0) ?? 0
        };

        return Ok(ApiResponse<object>.SuccessResponse(stats, "Estatísticas obtidas com sucesso"));
    }
}

