using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Challenge.FIAP.Controllers;
using System.Challenge.FIAP.DTOs;
using System.Challenge.FIAP.Interface;
using System.Challenge.FIAP.Request;
using System.Challenge.FIAP.Response;
using System.Challenge.FIAP.Data;
using System.Challenge.FIAP.Entities;
using Xunit;
using FluentAssertions;

namespace System.Challenge.FIAP.Tests.Controllers;

public class UserControllerTests
{
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<ILogger<UserController>> _mockLogger;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly UserController _controller;

    public UserControllerTests()
    {
        _mockUserService = new Mock<IUserService>();
        _mockLogger = new Mock<ILogger<UserController>>();
        _mockUserRepository = new Mock<IUserRepository>();
        _controller = new UserController(_mockUserService.Object, _mockLogger.Object, _mockUserRepository.Object);
    }

    #region CreateUser Tests

    [Fact]
    public async Task CreateUser_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            FullName = "João Silva",
            Email = "joao@test.com",
            Document = "12345678901",
            Role = "Student",
            StatusAccount = true
        };

        var expectedUser = new UserDto
        {
            IdUser = Guid.NewGuid(),
            FullName = "João Silva",
            Email = "joao@test.com",
            Document = "12345678901",
            Role = "Student"
        };

        var expectedResponse = ApiResponse<UserDto>.SuccessResponse(expectedUser, "Usuário criado com sucesso");
        _mockUserService.Setup(x => x.CreateUserAsync(It.IsAny<CreateUserRequest>()))
                       .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.CreateUser(request);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task CreateUser_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            FullName = "", // Invalid
            Email = "invalid-email", // Invalid
            Document = "", // Invalid
            Role = "Student",
            StatusAccount = true
        };

        _controller.ModelState.AddModelError("FullName", "Nome é obrigatório");
        _controller.ModelState.AddModelError("Email", "Email inválido");

        // Act
        var result = await _controller.CreateUser(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CreateUser_WithExistingEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            FullName = "João Silva",
            Email = "existing@test.com",
            Document = "12345678901",
            Role = "Student",
            StatusAccount = true
        };

        var errorResponse = ApiResponse<UserDto>.ErrorResponse("Email já cadastrado");
        _mockUserService.Setup(x => x.CreateUserAsync(It.IsAny<CreateUserRequest>()))
                       .ReturnsAsync(errorResponse);

        // Act
        var result = await _controller.CreateUser(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region GetUserById Tests

    [Fact]
    public async Task GetUserById_WithValidQuery_ShouldReturnOk()
    {
        // Arrange
        var query = "joao@test.com";
        var userId = Guid.NewGuid();
        
        var expectedUser = new UserDto
        {
            IdUser = userId,
            FullName = "João Silva",
            Email = "joao@test.com",
            Document = "12345678901",
            Role = "Student"
        };

        var mockUser = new User { IdUser = userId };
        _mockUserRepository.Setup(x => x.SearchUsersAsync(It.IsAny<string>()))
                          .ReturnsAsync(new List<User> { mockUser });
        
        _mockUserService.Setup(x => x.GetUserByIdAsync(It.IsAny<Guid>()))
                       .ReturnsAsync(expectedUser);

        // Act
        var result = await _controller.GetUserById(query);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetUserById_WithNonExistentQuery_ShouldReturnNotFound()
    {
        // Arrange
        var query = "nonexistent@test.com";
        
        _mockUserRepository.Setup(x => x.SearchUsersAsync(It.IsAny<string>()))
                          .ReturnsAsync(new List<User>());

        // Act
        var result = await _controller.GetUserById(query);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetUserById_WithServiceReturningNull_ShouldReturnNotFound()
    {
        // Arrange
        var query = "joao@test.com";
        var userId = Guid.NewGuid();
        
        var mockUser = new User { IdUser = userId };
        _mockUserRepository.Setup(x => x.SearchUsersAsync(It.IsAny<string>()))
                          .ReturnsAsync(new List<User> { mockUser });
        
        _mockUserService.Setup(x => x.GetUserByIdAsync(It.IsAny<Guid>()))
                       .ReturnsAsync((UserDto)null!);

        // Act
        var result = await _controller.GetUserById(query);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region GetAllUsers Tests

    [Fact]
    public async Task GetAllUsers_WithValidData_ShouldReturnOk()
    {
        // Arrange
        var users = new List<UserDto>
        {
            new UserDto
            {
                IdUser = Guid.NewGuid(),
                FullName = "João Silva",
                Email = "joao@test.com",
                Document = "12345678901",
                Role = "Student"
            }
        };

        _mockUserService.Setup(x => x.GetAllUsersAsync())
                       .ReturnsAsync(users);

        // Act
        var result = await _controller.GetAllUsers();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAllUsers_WithEmptyList_ShouldReturnOk()
    {
        // Arrange
        var users = new List<UserDto>();
        _mockUserService.Setup(x => x.GetAllUsersAsync())
                       .ReturnsAsync(users);

        // Act
        var result = await _controller.GetAllUsers();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAllUsers_WithServiceError_ShouldReturnOk()
    {
        // Arrange
        var users = new List<UserDto>();
        _mockUserService.Setup(x => x.GetAllUsersAsync())
                       .ReturnsAsync(users);

        // Act
        var result = await _controller.GetAllUsers();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region SearchUsers Tests

    [Fact]
    public async Task SearchUsers_WithValidQuery_ShouldReturnOk()
    {
        // Arrange
        var query = "João";
        var users = new List<UserDto>
        {
            new UserDto
            {
                IdUser = Guid.NewGuid(),
                FullName = "João Silva",
                Email = "joao@test.com",
                Document = "12345678901",
                Role = "Student"
            }
        };

        _mockUserService.Setup(x => x.SearchUsersAsync(It.IsAny<string>()))
                       .ReturnsAsync(users);

        // Act
        var result = await _controller.SearchUsers(query);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task SearchUsers_WithEmptyQuery_ShouldReturnBadRequest()
    {
        // Arrange
        var query = "";

        // Act
        var result = await _controller.SearchUsers(query);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task SearchUsers_WithNoResults_ShouldReturnOk()
    {
        // Arrange
        var query = "NonExistent";
        var users = new List<UserDto>();
        _mockUserService.Setup(x => x.SearchUsersAsync(It.IsAny<string>()))
                       .ReturnsAsync(users);

        // Act
        var result = await _controller.SearchUsers(query);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region UpdateUser Tests

    [Fact]
    public async Task UpdateUser_WithValidData_ShouldReturnOk()
    {
        // Arrange
        var query = "joao@test.com";
        var userId = Guid.NewGuid();
        var request = new UpdateUserRequest
        {
            FullName = "João Silva Atualizado",
            Email = "joao.updated@test.com",
            Document = "12345678901",
            Role = "Student"
        };

        var expectedUser = new UserDto
        {
            IdUser = userId,
            FullName = "João Silva Atualizado",
            Email = "joao.updated@test.com",
            Document = "12345678901",
            Role = "Student"
        };

        var mockUser = new Entities.User { IdUser = userId };
        _mockUserRepository.Setup(x => x.SearchUsersAsync(It.IsAny<string>()))
                          .ReturnsAsync(new List<Entities.User> { mockUser });
        
        _mockUserService.Setup(x => x.UpdateUserAsync(It.IsAny<Guid>(), It.IsAny<UpdateUserRequest>()))
                       .ReturnsAsync(expectedUser);

        // Act
        var result = await _controller.UpdateUser(query, request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task UpdateUser_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var query = "joao@test.com";
        var request = new UpdateUserRequest
        {
            FullName = "", // Invalid
            Email = "invalid-email", // Invalid
            Document = "", // Invalid
            Role = "Student"
        };

        _controller.ModelState.AddModelError("FullName", "Nome é obrigatório");
        _controller.ModelState.AddModelError("Email", "Email inválido");

        // Act
        var result = await _controller.UpdateUser(query, request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UpdateUser_WithNonExistentUser_ShouldReturnNotFound()
    {
        // Arrange
        var query = "nonexistent@test.com";
        var request = new UpdateUserRequest
        {
            FullName = "João Silva",
            Email = "joao@test.com",
            Document = "12345678901",
            Role = "Student"
        };

        _mockUserRepository.Setup(x => x.SearchUsersAsync(It.IsAny<string>()))
                          .ReturnsAsync(new List<Entities.User>());

        // Act
        var result = await _controller.UpdateUser(query, request);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region DeleteUser Tests

    [Fact]
    public async Task DeleteUser_WithValidQuery_ShouldReturnOk()
    {
        // Arrange
        var query = "joao@test.com";
        var userId = Guid.NewGuid();
        
        var mockUser = new Entities.User { IdUser = userId };
        _mockUserRepository.Setup(x => x.SearchUsersAsync(It.IsAny<string>()))
                          .ReturnsAsync(new List<Entities.User> { mockUser });
        
        _mockUserService.Setup(x => x.DeleteUserAsync(It.IsAny<Guid>()))
                       .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteUser(query);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task DeleteUser_WithNonExistentQuery_ShouldReturnNotFound()
    {
        // Arrange
        var query = "nonexistent@test.com";
        
        _mockUserRepository.Setup(x => x.SearchUsersAsync(It.IsAny<string>()))
                          .ReturnsAsync(new List<Entities.User>());

        // Act
        var result = await _controller.DeleteUser(query);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DeleteUser_WithServiceError_ShouldReturnNotFound()
    {
        // Arrange
        var query = "joao@test.com";
        var userId = Guid.NewGuid();
        
        var mockUser = new Entities.User { IdUser = userId };
        _mockUserRepository.Setup(x => x.SearchUsersAsync(It.IsAny<string>()))
                          .ReturnsAsync(new List<Entities.User> { mockUser });
        
        _mockUserService.Setup(x => x.DeleteUserAsync(It.IsAny<Guid>()))
                       .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteUser(query);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region CountUsers Tests

    [Fact]
    public async Task CountUsers_ShouldReturnOk()
    {
        // Arrange
        var users = new List<UserDto>
        {
            new UserDto { IdUser = Guid.NewGuid(), FullName = "João Silva", Email = "joao@test.com", Document = "12345678901", Role = "Student" },
            new UserDto { IdUser = Guid.NewGuid(), FullName = "Maria Santos", Email = "maria@test.com", Document = "98765432100", Role = "Admin" }
        };

        _mockUserService.Setup(x => x.GetAllUsersAsync())
                       .ReturnsAsync(users);

        // Act
        var result = await _controller.CountUsers();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion
}