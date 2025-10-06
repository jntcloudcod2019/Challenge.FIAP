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
public class EnrollmentController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;
    private readonly ILogger<EnrollmentController> _logger;

    public EnrollmentController(IEnrollmentService enrollmentService, ILogger<EnrollmentController> logger)
    {
        _enrollmentService = enrollmentService;
        _logger = logger;
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateEnrollment([FromBody] CreateEnrollmentRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<EnrollmentDto>.ErrorResponse("Dados inválidos"));
        }

        var result = await _enrollmentService.CreateEnrollmentAsync(request);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction("GetEnrollmentById", new { id = result.Data!.IdEnrollment }, result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<EnrollmentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllEnrollments()
    {
        var result = await _enrollmentService.GetAllEnrollmentsAsync();

        return Ok(result);
    }

    [HttpGet("student/{quey}")]
    [ProducesResponseType(typeof(ApiResponse<List<EnrollmentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEnrollmentsByStudent(string query)
    {

        var result = await _enrollmentService.SearchEnrollmentsAsync(null, null, null, query);

        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateEnrollment(Guid id, [FromBody] UpdateEnrollmentRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<EnrollmentDto>.ErrorResponse("Dados inválidos"));
        }

        var result = await _enrollmentService.UpdateEnrollmentAsync(id, request);

        if (!result.Success)
        {
            return result.Message.Contains("não encontrada") ? NotFound(result) : BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPatch("{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> PatchEnrollment(Guid id, [FromBody] UpdateEnrollmentRequest request)
    {
        var result = await _enrollmentService.UpdateEnrollmentAsync(id, request);

        if (!result.Success)
        {
            return result.Message.Contains("não encontrada") ? NotFound(result) : BadRequest(result);
        }

        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteEnrollment(Guid id)
    {
        var result = await _enrollmentService.DeleteEnrollmentAsync(id);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }
}

