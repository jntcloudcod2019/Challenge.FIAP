using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Challenge.FIAP.Controllers;
using System.Challenge.FIAP.DTOs;
using System.Challenge.FIAP.Interface;
using System.Challenge.FIAP.Request;
using System.Challenge.FIAP.Response;
using System.Security.Claims;
using Xunit;
using FluentAssertions;

namespace System.Challenge.FIAP.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly Mock<ILogger<AuthController>> _mockLogger;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _mockAuthService = new Mock<IAuthService>();
        _mockLogger = new Mock<ILogger<AuthController>>();
        _controller = new AuthController(_mockAuthService.Object, _mockLogger.Object);
    }

    #region Register Tests

    [Fact]
    public async Task Register_WithValidData_ShouldReturnOk()
    {
        // Arrange
        var request = new RegisterUserRequest
        {
            FullName = "João Silva",
            Email = "joao@test.com",
            Document = "12345678901",
            Password = "Password123!",
            Role = "Student"
        };

        var expectedResponse = new AuthResponse
        {
            Success = true,
            Message = "Usuário registrado com sucesso",
            Token = "jwt-token-here",
            User = new UserDto
            {
                IdUser = Guid.NewGuid(),
                FullName = "João Silva",
                Email = "joao@test.com",
                Document = "12345678901",
                Role = "Student"
            }
        };

        _mockAuthService.Setup(x => x.RegisterAsync(It.IsAny<RegisterUserRequest>()))
                       .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.Register(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task Register_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new RegisterUserRequest
        {
            FullName = "", // Invalid
            Email = "invalid-email", // Invalid
            Document = "", // Invalid
            Password = "123", // Invalid
            Role = "Student"
        };

        _controller.ModelState.AddModelError("FullName", "Nome é obrigatório");
        _controller.ModelState.AddModelError("Email", "Email inválido");

        // Act
        var result = await _controller.Register(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        var response = badRequestResult!.Value as AuthResponse;
        response!.Success.Should().BeFalse();
        response.Message.Should().Be("Dados inválidos");
    }

    [Fact]
    public async Task Register_WithExistingEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new RegisterUserRequest
        {
            FullName = "João Silva",
            Email = "existing@test.com",
            Document = "12345678901",
            Password = "Password123!",
            Role = "Student"
        };

        var errorResponse = new AuthResponse
        {
            Success = false,
            Message = "Email já cadastrado"
        };

        _mockAuthService.Setup(x => x.RegisterAsync(It.IsAny<RegisterUserRequest>()))
                       .ReturnsAsync(errorResponse);

        // Act
        var result = await _controller.Register(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        var response = badRequestResult!.Value as AuthResponse;
        response!.Success.Should().BeFalse();
        response.Message.Should().Be("Email já cadastrado");
    }

    #endregion

    #region Login Tests

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnOk()
    {
        // Arrange
        var request = new LoginUserRequest
        {
            Email = "joao@test.com",
            Password = "Password123!"
        };

        var expectedResponse = new AuthResponse
        {
            Success = true,
            Message = "Login realizado com sucesso",
            Token = "jwt-token-here",
            User = new UserDto
            {
                IdUser = Guid.NewGuid(),
                FullName = "João Silva",
                Email = "joao@test.com",
                Document = "12345678901",
                Role = "Student"
            }
        };

        _mockAuthService.Setup(x => x.LoginAsync(It.IsAny<LoginUserRequest>()))
                       .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.Login(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new LoginUserRequest
        {
            Email = "joao@test.com",
            Password = "WrongPassword"
        };

        var errorResponse = new AuthResponse
        {
            Success = false,
            Message = "Credenciais inválidas"
        };

        _mockAuthService.Setup(x => x.LoginAsync(It.IsAny<LoginUserRequest>()))
                       .ReturnsAsync(errorResponse);

        // Act
        var result = await _controller.Login(request);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
        var unauthorizedResult = result as UnauthorizedObjectResult;
        var response = unauthorizedResult!.Value as AuthResponse;
        response!.Success.Should().BeFalse();
        response.Message.Should().Be("Credenciais inválidas");
    }

    [Fact]
    public async Task Login_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new LoginUserRequest
        {
            Email = "", // Invalid
            Password = "" // Invalid
        };

        _controller.ModelState.AddModelError("Email", "Email é obrigatório");
        _controller.ModelState.AddModelError("Password", "Senha é obrigatória");

        // Act
        var result = await _controller.Login(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        var response = badRequestResult!.Value as AuthResponse;
        response!.Success.Should().BeFalse();
        response.Message.Should().Be("Dados inválidos");
    }

    #endregion

    #region GetCurrentUser Tests

    [Fact]
    public async Task GetCurrentUser_WithValidToken_ShouldReturnOk()
    {
        // Arrange
        var email = "joao@test.com";
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, email)
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
            {
                User = claimsPrincipal
            }
        };

        var expectedUser = new UserDto
        {
            IdUser = Guid.NewGuid(),
            FullName = "João Silva",
            Email = email,
            Document = "12345678901",
            Role = "Student"
        };

        _mockAuthService.Setup(x => x.GetCurrentUserAsync(It.IsAny<string>()))
                       .ReturnsAsync(expectedUser);

        // Act
        var result = await _controller.GetCurrentUser();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as ApiResponse<UserDto>;
        response!.Success.Should().BeTrue();
        response.Data.Should().BeEquivalentTo(expectedUser);
    }

    [Fact]
    public async Task GetCurrentUser_WithoutEmailClaim_ShouldReturnUnauthorized()
    {
        // Arrange
        var identity = new ClaimsIdentity(new List<Claim>(), "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
            {
                User = claimsPrincipal
            }
        };

        // Act
        var result = await _controller.GetCurrentUser();

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
        var unauthorizedResult = result as UnauthorizedObjectResult;
        var response = unauthorizedResult!.Value as ApiResponse<UserDto>;
        response!.Success.Should().BeFalse();
        response.Message.Should().Be("Usuário não autenticado");
    }

    [Fact]
    public async Task GetCurrentUser_WithNonExistentUser_ShouldReturnNotFound()
    {
        // Arrange
        var email = "nonexistent@test.com";
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, email)
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext
            {
                User = claimsPrincipal
            }
        };

        _mockAuthService.Setup(x => x.GetCurrentUserAsync(It.IsAny<string>()))
                       .ReturnsAsync((UserDto)null!);

        // Act
        var result = await _controller.GetCurrentUser();

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        var response = notFoundResult!.Value as ApiResponse<UserDto>;
        response!.Success.Should().BeFalse();
        response.Message.Should().Be("Usuário não encontrado");
    }

    #endregion
}

