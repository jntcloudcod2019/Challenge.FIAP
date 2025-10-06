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

public class ClassControllerTests
{
    private readonly Mock<IClassService> _mockClassService;
    private readonly Mock<ILogger<ClassController>> _mockLogger;
    private readonly ClassController _controller;

    public ClassControllerTests()
    {
        _mockClassService = new Mock<IClassService>();
        _mockLogger = new Mock<ILogger<ClassController>>();
        _controller = new ClassController(_mockClassService.Object, _mockLogger.Object);
    }

    #region CreateClass Tests

    [Fact]
    public async Task CreateClass_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var request = new CreateClassRequest
        {
            Name = "Turma de Matemática",
            Description = "Aulas de matemática avançada",
            Room = "Sala 101"
        };

        var expectedClass = new ClassDto
        {
            IdClass = Guid.NewGuid(),
            ClassCode = "CLS001",
            Name = "Turma de Matemática",
            Description = "Aulas de matemática avançada",
            Room = "Sala 101",
            Capacity = 50,
            Status = "Open",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var expectedResponse = ApiResponse<ClassDto>.SuccessResponse(expectedClass, "Turma criada com sucesso");
        _mockClassService.Setup(x => x.CreateClassAsync(It.IsAny<CreateClassRequest>()))
                        .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.CreateClass(request);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task CreateClass_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateClassRequest
        {
            Name = "", // Invalid
            Description = "Aulas de matemática avançada",
            Room = "Sala 101"
        };

        _controller.ModelState.AddModelError("Name", "Nome é obrigatório");

        // Act
        var result = await _controller.CreateClass(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CreateClass_WithExistingName_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateClassRequest
        {
            Name = "Turma Existente",
            Description = "Aulas de matemática avançada",
            Room = "Sala 101"
        };

        var errorResponse = ApiResponse<ClassDto>.ErrorResponse("Nome da turma já existe");
        _mockClassService.Setup(x => x.CreateClassAsync(It.IsAny<CreateClassRequest>()))
                        .ReturnsAsync(errorResponse);

        // Act
        var result = await _controller.CreateClass(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region GetClassByCode Tests

    [Fact]
    public async Task GetClassByCode_WithValidCode_ShouldReturnOk()
    {
        // Arrange
        var classCode = "CLS001";
        var expectedClass = new ClassDto
        {
            IdClass = Guid.NewGuid(),
            ClassCode = classCode,
            Name = "Turma de Matemática",
            Description = "Aulas de matemática avançada",
            Room = "Sala 101",
            Capacity = 50,
            Status = "Open",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockClassService.Setup(x => x.GetClassByCodeAsync(It.IsAny<string>()))
                        .ReturnsAsync(expectedClass);

        // Act
        var result = await _controller.GetClassByCode(classCode);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetClassByCode_WithNonExistentCode_ShouldReturnNotFound()
    {
        // Arrange
        var classCode = "CLS999";
        _mockClassService.Setup(x => x.GetClassByCodeAsync(It.IsAny<string>()))
                        .ReturnsAsync((ClassDto)null!);

        // Act
        var result = await _controller.GetClassByCode(classCode);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetClassByCode_WithEmptyCode_ShouldReturnNotFound()
    {
        // Arrange
        var classCode = "";
        _mockClassService.Setup(x => x.GetClassByCodeAsync(It.IsAny<string>()))
                        .ReturnsAsync((ClassDto)null!);

        // Act
        var result = await _controller.GetClassByCode(classCode);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion


    #region UpdateClass Tests

    [Fact]
    public async Task UpdateClass_WithValidData_ShouldReturnOk()
    {
        // Arrange
        var request = new UpdateClassRequest
        {
            ClassCode = "CLS001",
            Room = "Sala 201",
            Status = "Closed"
        };

        var expectedClass = new ClassDto
        {
            IdClass = Guid.NewGuid(),
            ClassCode = "CLS001",
            Name = "Turma de Matemática",
            Description = "Aulas de matemática avançada",
            Room = "Sala 201",
            Capacity = 50,
            Status = "Closed",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockClassService.Setup(x => x.UpdateClassAsync(It.IsAny<string>(), It.IsAny<UpdateClassRequest>()))
                        .ReturnsAsync(expectedClass);

        // Act
        var result = await _controller.UpdateClass(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task UpdateClass_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new UpdateClassRequest
        {
            ClassCode = "CLS001",
            Room = "", // Invalid
            Status = "InvalidStatus" // Invalid
        };

        _controller.ModelState.AddModelError("Room", "Sala é obrigatória");
        _controller.ModelState.AddModelError("Status", "Status inválido");

        // Act
        var result = await _controller.UpdateClass(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UpdateClass_WithNonExistentCode_ShouldReturnNotFound()
    {
        // Arrange
        var request = new UpdateClassRequest
        {
            ClassCode = "CLS999",
            Room = "Sala 201",
            Status = "Closed"
        };

        _mockClassService.Setup(x => x.UpdateClassAsync(It.IsAny<string>(), It.IsAny<UpdateClassRequest>()))
                        .ReturnsAsync((ClassDto)null!);

        // Act
        var result = await _controller.UpdateClass(request);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region DeleteClass Tests

    [Fact]
    public async Task DeleteClass_WithValidCode_ShouldReturnOk()
    {
        // Arrange
        var classCode = "CLS001";
        _mockClassService.Setup(x => x.DeleteClassAsync(It.IsAny<string>()))
                        .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteClass(classCode);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task DeleteClass_WithNonExistentCode_ShouldReturnNotFound()
    {
        // Arrange
        var classCode = "CLS999";
        _mockClassService.Setup(x => x.DeleteClassAsync(It.IsAny<string>()))
                        .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteClass(classCode);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DeleteClass_WithEmptyCode_ShouldReturnNotFound()
    {
        // Arrange
        var classCode = "";
        _mockClassService.Setup(x => x.DeleteClassAsync(It.IsAny<string>()))
                        .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteClass(classCode);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion
}