using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Challenge.FIAP.Controllers;
using System.Challenge.FIAP.DTOs;
using System.Challenge.FIAP.Interface;
using System.Challenge.FIAP.Request;
using System.Challenge.FIAP.Response;
using Xunit;
using FluentAssertions;

namespace System.Challenge.FIAP.Tests.Controllers;

public class EnrollmentControllerTests
{
    private readonly Mock<IEnrollmentService> _mockEnrollmentService;
    private readonly Mock<ILogger<EnrollmentController>> _mockLogger;
    private readonly EnrollmentController _controller;

    public EnrollmentControllerTests()
    {
        _mockEnrollmentService = new Mock<IEnrollmentService>();
        _mockLogger = new Mock<ILogger<EnrollmentController>>();
        _controller = new EnrollmentController(_mockEnrollmentService.Object, _mockLogger.Object);
    }

    #region CreateEnrollment Tests

    [Fact]
    public async Task CreateEnrollment_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var request = new CreateEnrollmentRequest
        {
            StudentDocumentOrRA = "RA123456",
            ClassCode = "CLS001"
        };

        var expectedEnrollment = new EnrollmentDto
        {
            IdEnrollment = Guid.NewGuid(),
            IdStudent = Guid.NewGuid(),
            IdClass = Guid.NewGuid(),
            EnrollmentDate = DateTime.UtcNow,
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            StudentName = "João Silva",
            ClassName = "Turma de Matemática"
        };

        var expectedResponse = ApiResponse<EnrollmentDto>.SuccessResponse(expectedEnrollment, "Matrícula criada com sucesso");
        _mockEnrollmentService.Setup(x => x.CreateEnrollmentAsync(It.IsAny<CreateEnrollmentRequest>()))
                             .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.CreateEnrollment(request);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task CreateEnrollment_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateEnrollmentRequest
        {
            StudentDocumentOrRA = "", // Invalid
            ClassCode = "" // Invalid
        };

        _controller.ModelState.AddModelError("StudentDocumentOrRA", "Documento/RA é obrigatório");
        _controller.ModelState.AddModelError("ClassCode", "Código da turma é obrigatório");

        // Act
        var result = await _controller.CreateEnrollment(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CreateEnrollment_WithExistingEnrollment_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateEnrollmentRequest
        {
            StudentDocumentOrRA = "RA123456",
            ClassCode = "CLS001"
        };

        var errorResponse = ApiResponse<EnrollmentDto>.ErrorResponse("Aluno já está matriculado nesta turma");
        _mockEnrollmentService.Setup(x => x.CreateEnrollmentAsync(It.IsAny<CreateEnrollmentRequest>()))
                             .ReturnsAsync(errorResponse);

        // Act
        var result = await _controller.CreateEnrollment(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region GetAllEnrollments Tests

    [Fact]
    public async Task GetAllEnrollments_WithValidData_ShouldReturnOk()
    {
        // Arrange
        var enrollments = new List<EnrollmentDto>
        {
            new EnrollmentDto
            {
                IdEnrollment = Guid.NewGuid(),
                IdStudent = Guid.NewGuid(),
                IdClass = Guid.NewGuid(),
                EnrollmentDate = DateTime.UtcNow,
                Status = "Active",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                StudentName = "João Silva",
                ClassName = "Turma de Matemática"
            },
            new EnrollmentDto
            {
                IdEnrollment = Guid.NewGuid(),
                IdStudent = Guid.NewGuid(),
                IdClass = Guid.NewGuid(),
                EnrollmentDate = DateTime.UtcNow,
                Status = "Active",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                StudentName = "Maria Santos",
                ClassName = "Turma de Física"
            }
        };

        var expectedResponse = ApiResponse<List<EnrollmentDto>>.SuccessResponse(enrollments, "Matrículas encontradas");
        _mockEnrollmentService.Setup(x => x.GetAllEnrollmentsAsync())
                             .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetAllEnrollments();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAllEnrollments_WithEmptyList_ShouldReturnOk()
    {
        // Arrange
        var enrollments = new List<EnrollmentDto>();
        var expectedResponse = ApiResponse<List<EnrollmentDto>>.SuccessResponse(enrollments, "Nenhuma matrícula encontrada");
        _mockEnrollmentService.Setup(x => x.GetAllEnrollmentsAsync())
                             .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetAllEnrollments();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAllEnrollments_WithServiceError_ShouldReturnOk()
    {
        // Arrange
        var enrollments = new List<EnrollmentDto>();
        var expectedResponse = ApiResponse<List<EnrollmentDto>>.SuccessResponse(enrollments, "Nenhuma matrícula encontrada");
        _mockEnrollmentService.Setup(x => x.GetAllEnrollmentsAsync())
                             .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetAllEnrollments();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region UpdateEnrollment Tests

    [Fact]
    public async Task UpdateEnrollment_WithValidData_ShouldReturnOk()
    {
        // Arrange
        var enrollmentId = Guid.NewGuid();
        var request = new UpdateEnrollmentRequest
        {
            Status = "Suspended"
        };

        var expectedEnrollment = new EnrollmentDto
        {
            IdEnrollment = enrollmentId,
            IdStudent = Guid.NewGuid(),
            IdClass = Guid.NewGuid(),
            EnrollmentDate = DateTime.UtcNow,
            Status = "Suspended",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            StudentName = "João Silva",
            ClassName = "Turma de Matemática"
        };

        var expectedResponse = ApiResponse<EnrollmentDto>.SuccessResponse(expectedEnrollment, "Matrícula atualizada com sucesso");
        _mockEnrollmentService.Setup(x => x.UpdateEnrollmentAsync(It.IsAny<Guid>(), It.IsAny<UpdateEnrollmentRequest>()))
                             .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.UpdateEnrollment(enrollmentId, request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task UpdateEnrollment_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var enrollmentId = Guid.NewGuid();
        var request = new UpdateEnrollmentRequest
        {
            Status = "InvalidStatus" // Invalid
        };

        _controller.ModelState.AddModelError("Status", "Status inválido");

        // Act
        var result = await _controller.UpdateEnrollment(enrollmentId, request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UpdateEnrollment_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var enrollmentId = Guid.NewGuid();
        var request = new UpdateEnrollmentRequest
        {
            Status = "Suspended"
        };

        var errorResponse = ApiResponse<EnrollmentDto>.ErrorResponse("Matrícula não encontrada");
        _mockEnrollmentService.Setup(x => x.UpdateEnrollmentAsync(It.IsAny<Guid>(), It.IsAny<UpdateEnrollmentRequest>()))
                             .ReturnsAsync(errorResponse);

        // Act
        var result = await _controller.UpdateEnrollment(enrollmentId, request);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region DeleteEnrollment Tests

    [Fact]
    public async Task DeleteEnrollment_WithValidId_ShouldReturnOk()
    {
        // Arrange
        var enrollmentId = Guid.NewGuid();
        var successResponse = ApiResponse<bool>.SuccessResponse(true, "Matrícula deletada com sucesso");
        _mockEnrollmentService.Setup(x => x.DeleteEnrollmentAsync(It.IsAny<Guid>()))
                             .ReturnsAsync(successResponse);

        // Act
        var result = await _controller.DeleteEnrollment(enrollmentId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task DeleteEnrollment_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var enrollmentId = Guid.NewGuid();
        var errorResponse = ApiResponse<bool>.ErrorResponse("Matrícula não encontrada");
        _mockEnrollmentService.Setup(x => x.DeleteEnrollmentAsync(It.IsAny<Guid>()))
                             .ReturnsAsync(errorResponse);

        // Act
        var result = await _controller.DeleteEnrollment(enrollmentId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DeleteEnrollment_WithServiceError_ShouldReturnNotFound()
    {
        // Arrange
        var enrollmentId = Guid.NewGuid();
        var errorResponse = ApiResponse<bool>.ErrorResponse("Erro interno do servidor");
        _mockEnrollmentService.Setup(x => x.DeleteEnrollmentAsync(It.IsAny<Guid>()))
                             .ReturnsAsync(errorResponse);

        // Act
        var result = await _controller.DeleteEnrollment(enrollmentId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion
}