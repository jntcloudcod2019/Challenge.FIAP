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

public class StudentControllerTests
{
    private readonly Mock<IStudentService> _mockStudentService;
    private readonly Mock<ILogger<StudentController>> _mockLogger;
    private readonly StudentController _controller;

    public StudentControllerTests()
    {
        _mockStudentService = new Mock<IStudentService>();
        _mockLogger = new Mock<ILogger<StudentController>>();
        _controller = new StudentController(_mockStudentService.Object, _mockLogger.Object);
    }

    #region CreateStudent Tests

    [Fact]
    public async Task CreateStudent_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var request = new CreateStudentRequest
        {
            RegistrationNumber = "RA123456",
            Cpf = "12345678901",
            FullName = "João Silva",
            Email = "joao@test.com",
            BirthDate = DateTime.Now.AddYears(-20),
            Address = "Rua das Flores, 123",
            PhoneNumber = "11999999999"
        };

        var expectedStudent = new StudentDto
        {
            IdStudent = Guid.NewGuid(),
            RegistrationNumber = "RA123456",
            Cpf = "12345678901",
            FullName = "João Silva",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var expectedResponse = ApiResponse<StudentDto>.SuccessResponse(expectedStudent, "Estudante criado com sucesso");
        _mockStudentService.Setup(x => x.CreateStudentAsync(It.IsAny<CreateStudentRequest>()))
                          .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.CreateStudent(request);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task CreateStudent_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateStudentRequest
        {
            RegistrationNumber = "", // Invalid
            Cpf = "123", // Invalid
            FullName = "", // Invalid
            Email = "invalid-email", // Invalid
            BirthDate = DateTime.Now.AddYears(-20),
            Address = "Rua das Flores, 123",
            PhoneNumber = "11999999999"
        };

        _controller.ModelState.AddModelError("RegistrationNumber", "RA é obrigatório");
        _controller.ModelState.AddModelError("Cpf", "CPF inválido");

        // Act
        var result = await _controller.CreateStudent(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CreateStudent_WithExistingCpf_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateStudentRequest
        {
            RegistrationNumber = "RA123456",
            Cpf = "12345678901",
            FullName = "João Silva",
            Email = "joao@test.com",
            BirthDate = DateTime.Now.AddYears(-20),
            Address = "Rua das Flores, 123",
            PhoneNumber = "11999999999"
        };

        var errorResponse = ApiResponse<StudentDto>.ErrorResponse("CPF já cadastrado");
        _mockStudentService.Setup(x => x.CreateStudentAsync(It.IsAny<CreateStudentRequest>()))
                          .ReturnsAsync(errorResponse);

        // Act
        var result = await _controller.CreateStudent(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region GetStudentByRegistrationNumber Tests

    [Fact]
    public async Task GetStudentByRegistrationNumber_WithValidRa_ShouldReturnOk()
    {
        // Arrange
        var registrationNumber = "RA123456";
        var expectedStudent = new StudentDto
        {
            IdStudent = Guid.NewGuid(),
            RegistrationNumber = registrationNumber,
            Cpf = "12345678901",
            FullName = "João Silva",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var expectedResponse = ApiResponse<StudentDto>.SuccessResponse(expectedStudent, "Estudante encontrado");
        _mockStudentService.Setup(x => x.GetStudentByRegistrationNumberAsync(It.IsAny<string>()))
                          .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetStudentByRegistrationNumber(registrationNumber);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetStudentByRegistrationNumber_WithNonExistentRa_ShouldReturnNotFound()
    {
        // Arrange
        var registrationNumber = "RA999999";
        var errorResponse = ApiResponse<StudentDto>.ErrorResponse("Estudante não encontrado");
        _mockStudentService.Setup(x => x.GetStudentByRegistrationNumberAsync(It.IsAny<string>()))
                          .ReturnsAsync(errorResponse);

        // Act
        var result = await _controller.GetStudentByRegistrationNumber(registrationNumber);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetStudentByRegistrationNumber_WithEmptyRa_ShouldReturnNotFound()
    {
        // Arrange
        var registrationNumber = "";
        var errorResponse = ApiResponse<StudentDto>.ErrorResponse("Estudante não encontrado");
        _mockStudentService.Setup(x => x.GetStudentByRegistrationNumberAsync(It.IsAny<string>()))
                          .ReturnsAsync(errorResponse);

        // Act
        var result = await _controller.GetStudentByRegistrationNumber(registrationNumber);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region FindStudent Tests

    [Fact]
    public async Task FindStudent_WithValidQuery_ShouldReturnOk()
    {
        // Arrange
        var query = "João";
        var expectedStudent = new StudentDto
        {
            IdStudent = Guid.NewGuid(),
            RegistrationNumber = "RA123456",
            Cpf = "12345678901",
            FullName = "João Silva",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var expectedResponse = ApiResponse<StudentDto>.SuccessResponse(expectedStudent, "Estudante encontrado");
        _mockStudentService.Setup(x => x.FindStudentAsync(It.IsAny<string>()))
                          .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.FindStudent(query);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task FindStudent_WithEmptyQuery_ShouldReturnNotFound()
    {
        // Arrange
        var query = "";
        var errorResponse = ApiResponse<StudentDto>.ErrorResponse("Estudante não encontrado");
        _mockStudentService.Setup(x => x.FindStudentAsync(It.IsAny<string>()))
                          .ReturnsAsync(errorResponse);

        // Act
        var result = await _controller.FindStudent(query);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task FindStudent_WithNonExistentQuery_ShouldReturnNotFound()
    {
        // Arrange
        var query = "NonExistent";
        var errorResponse = ApiResponse<StudentDto>.ErrorResponse("Estudante não encontrado");
        _mockStudentService.Setup(x => x.FindStudentAsync(It.IsAny<string>()))
                          .ReturnsAsync(errorResponse);

        // Act
        var result = await _controller.FindStudent(query);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region GetAllStudents Tests

    [Fact]
    public async Task GetAllStudents_WithValidData_ShouldReturnOk()
    {
        // Arrange
        var students = new List<StudentDto>
        {
            new StudentDto
            {
                IdStudent = Guid.NewGuid(),
                RegistrationNumber = "RA123456",
                Cpf = "12345678901",
                FullName = "João Silva",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        var expectedResponse = ApiResponse<List<StudentDto>>.SuccessResponse(students, "Estudantes encontrados");
        _mockStudentService.Setup(x => x.GetAllStudentsAsync())
                          .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetAllStudents();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAllStudents_WithEmptyList_ShouldReturnOk()
    {
        // Arrange
        var students = new List<StudentDto>();
        var expectedResponse = ApiResponse<List<StudentDto>>.SuccessResponse(students, "Nenhum estudante encontrado");
        _mockStudentService.Setup(x => x.GetAllStudentsAsync())
                          .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetAllStudents();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAllStudents_WithServiceError_ShouldReturnBadRequest()
    {
        // Arrange
        var errorResponse = ApiResponse<List<StudentDto>>.ErrorResponse("Erro interno do servidor");
        _mockStudentService.Setup(x => x.GetAllStudentsAsync())
                          .ReturnsAsync(errorResponse);

        // Act
        var result = await _controller.GetAllStudents();

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region SearchStudents Tests

    [Fact]
    public async Task SearchStudents_WithValidQuery_ShouldReturnOk()
    {
        // Arrange
        var query = "João";
        var students = new List<StudentDto>
        {
            new StudentDto
            {
                IdStudent = Guid.NewGuid(),
                RegistrationNumber = "RA123456",
                Cpf = "12345678901",
                FullName = "João Silva",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        var expectedResponse = ApiResponse<List<StudentDto>>.SuccessResponse(students, "Busca realizada com sucesso");
        _mockStudentService.Setup(x => x.GetAllStudentsAsync())
                          .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.SearchStudents(query);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task SearchStudents_WithEmptyQuery_ShouldReturnBadRequest()
    {
        // Arrange
        var query = "";

        // Act
        var result = await _controller.SearchStudents(query);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task SearchStudents_WithNoResults_ShouldReturnOk()
    {
        // Arrange
        var query = "NonExistent";
        var students = new List<StudentDto>();
        var expectedResponse = ApiResponse<List<StudentDto>>.SuccessResponse(students, "Nenhum resultado encontrado");
        _mockStudentService.Setup(x => x.GetAllStudentsAsync())
                          .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.SearchStudents(query);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region UpdateStudent Tests

    [Fact]
    public async Task UpdateStudent_WithValidData_ShouldReturnOk()
    {
        // Arrange
        var query = "RA123456";
        var studentId = Guid.NewGuid();
        var request = new UpdateStudentRequest
        {
            FullName = "João Silva Atualizado",
            BirthDate = DateTime.Now.AddYears(-21),
            Address = "Rua das Flores Atualizada, 456",
            PhoneNumber = "11888888888"
        };

        var expectedStudent = new StudentDto
        {
            IdStudent = studentId,
            RegistrationNumber = "RA123456",
            Cpf = "12345678901",
            FullName = "João Silva Atualizado",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var expectedResponse = ApiResponse<StudentDto>.SuccessResponse(expectedStudent, "Estudante atualizado com sucesso");
        _mockStudentService.Setup(x => x.FindStudentAsync(It.IsAny<string>()))
                          .ReturnsAsync(expectedResponse);
        _mockStudentService.Setup(x => x.UpdateStudentAsync(It.IsAny<Guid>(), It.IsAny<UpdateStudentRequest>()))
                          .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.UpdateStudent(query, request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task UpdateStudent_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var query = "RA123456";
        var request = new UpdateStudentRequest
        {
            FullName = "", // Invalid
            BirthDate = DateTime.Now.AddYears(-21),
            Address = "", // Invalid
            PhoneNumber = "invalid-phone" // Invalid
        };

        _controller.ModelState.AddModelError("FullName", "Nome é obrigatório");
        _controller.ModelState.AddModelError("Address", "Endereço é obrigatório");

        // Act
        var result = await _controller.UpdateStudent(query, request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UpdateStudent_WithNonExistentQuery_ShouldReturnNotFound()
    {
        // Arrange
        var query = "RA999999";
        var request = new UpdateStudentRequest
        {
            FullName = "João Silva",
            BirthDate = DateTime.Now.AddYears(-21),
            Address = "Rua das Flores, 123",
            PhoneNumber = "11999999999"
        };

        var errorResponse = ApiResponse<StudentDto>.ErrorResponse("Estudante não encontrado");
        _mockStudentService.Setup(x => x.FindStudentAsync(It.IsAny<string>()))
                          .ReturnsAsync(errorResponse);

        // Act
        var result = await _controller.UpdateStudent(query, request);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region DeleteStudent Tests

    [Fact]
    public async Task DeleteStudent_WithValidQuery_ShouldReturnOk()
    {
        // Arrange
        var query = "João";
        var successResponse = ApiResponse<bool>.SuccessResponse(true, "Estudante deletado com sucesso");
        _mockStudentService.Setup(x => x.DeleteStudentByQueryAsync(It.IsAny<string>()))
                          .ReturnsAsync(successResponse);

        // Act
        var result = await _controller.DeleteStudent(query);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task DeleteStudent_WithNonExistentQuery_ShouldReturnNotFound()
    {
        // Arrange
        var query = "NonExistent";
        var errorResponse = ApiResponse<bool>.ErrorResponse("Estudante não encontrado");
        _mockStudentService.Setup(x => x.DeleteStudentByQueryAsync(It.IsAny<string>()))
                          .ReturnsAsync(errorResponse);

        // Act
        var result = await _controller.DeleteStudent(query);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DeleteStudent_WithEmptyQuery_ShouldReturnNotFound()
    {
        // Arrange
        var query = "";
        var errorResponse = ApiResponse<bool>.ErrorResponse("Estudante não encontrado");
        _mockStudentService.Setup(x => x.DeleteStudentByQueryAsync(It.IsAny<string>()))
                          .ReturnsAsync(errorResponse);

        // Act
        var result = await _controller.DeleteStudent(query);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion
}