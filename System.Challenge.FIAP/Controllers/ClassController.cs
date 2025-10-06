using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Challenge.FIAP.DTOs;
using System.Challenge.FIAP.Interface;
using System.Challenge.FIAP.Request;
using System.Challenge.FIAP.Response;

namespace System.Challenge.FIAP.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClassController : ControllerBase
{
    private readonly IClassService _classService;
    private readonly ILogger<ClassController> _logger;

    public ClassController(IClassService classService, ILogger<ClassController> logger)
    {
        _classService = classService;
        _logger = logger;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<ClassDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ClassDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateClass([FromBody] CreateClassRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<ClassDto>.ErrorResponse("Dados inválidos"));
        }

        var result = await _classService.CreateClassAsync(request);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetClassByCode), new { code = result.Data?.ClassCode }, result);
    }

    [HttpGet("code/{code}")]
    [ProducesResponseType(typeof(ApiResponse<ClassDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ClassDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetClassByCode(string code)
    {
        var result = await _classService.GetClassByCodeAsync(code);

        if (result == null)
        {
            return NotFound(new { 
                success = false, 
                message = "Turma não encontrada", 
                errors = new List<string>(),
                data = (ClassDto?)null
            });
        }

        return Ok(new { 
            success = true, 
            message = "Turma encontrada",
            data = result
        });
    }

    [HttpPut]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<ClassDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ClassDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ClassDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateClass([FromBody] UpdateClassRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<ClassDto>.ErrorResponse("Dados inválidos"));
        }

        var result = await _classService.UpdateClassAsync(request.ClassCode, request);

        if (result == null)
        {
            return NotFound(new { 
                success = false, 
                message = "Turma não encontrada", 
                errors = new List<string>(),
                data = (ClassDto?)null
            });
        }

        return Ok(new { 
            success = true, 
            message = "Turma atualizada com sucesso",
            data = result
        });
    }

    [HttpDelete("{classCode}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteClass(string classCode)
    {
        var result = await _classService.DeleteClassAsync(classCode);

        if (!result)
        {
            return NotFound(new { 
                success = false, 
                message = "Turma não encontrada", 
                errors = new List<string>(),
                data = false
            });
        }

        return Ok(new { 
            success = true, 
            message = "Turma deletada com sucesso",
            data = true
        });
    }

}

