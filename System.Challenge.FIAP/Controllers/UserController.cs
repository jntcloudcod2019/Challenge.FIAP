using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Challenge.FIAP.Interface;
using System.Challenge.FIAP.Request;
using System.Challenge.FIAP.Response;
using System.Challenge.FIAP.Data;
using System.Challenge.FIAP.DTOs;

namespace System.Challenge.FIAP.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;
    private readonly IUserRepository _userRepository;
    public UserController(IUserService userService, ILogger<UserController> logger, IUserRepository userRepository)
    {
        _userService = userService;
        _logger = logger;
        _userRepository = userRepository;
    }

    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<UserDto>.ErrorResponse("Dados inválidos"));
        }

        var result = await _userService.CreateUserAsync(request);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetUserById), new { id = result.Data!.IdUser }, new { 
            success = true, 
            message = result.Message,
            data = result.Data
        });
    }

    [HttpGet("{query}")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(string query)
    {
        var users = await _userRepository.SearchUsersAsync(query);
        var user = users.FirstOrDefault();
        
        if (user == null)
        {
            return NotFound(new { 
                success = false, 
                message = "Usuário não encontrado", 
                errors = new List<string>(),
                data = (UserDto?)null
            });
        }
        
        var userDto = await _userService.GetUserByIdAsync(user.IdUser);

        if (userDto == null)
        {
            return NotFound(new { 
                success = false, 
                message = "Usuário não encontrado", 
                errors = new List<string>(),
                data = (UserDto?)null
            });
        }

        return Ok(new { 
            success = true, 
            message = "Usuário encontrado",
            data = userDto
        });
    }

    [HttpPut("{query}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateUser(string query, [FromBody] UpdateUserRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<UserDto>.ErrorResponse("Dados inválidos"));
        }

        var users = await _userRepository.SearchUsersAsync(query);
        var user = users.FirstOrDefault();
        
        if (user == null)
        {
            return NotFound(new { 
                success = false, 
                message = "Usuário não encontrado", 
                errors = new List<string>(),
                data = (UserDto?)null
            });
        }
        
        try
        {
            var updatedUser = await _userService.UpdateUserAsync(user.IdUser, request);

            if (updatedUser == null)
            {
                return NotFound(new { 
                    success = false, 
                    message = "Usuário não encontrado", 
                    errors = new List<string>(),
                    data = (UserDto?)null
                });
            }

            return Ok(new { 
                success = true, 
                message = "Usuário atualizado com sucesso",
                data = updatedUser
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { 
                success = false, 
                message = ex.Message, 
                errors = new List<string>(),
                data = (UserDto?)null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar usuário");
            return BadRequest(new { 
                success = false, 
                message = "Erro ao atualizar usuário", 
                errors = new List<string> { ex.Message },
                data = (UserDto?)null
            });
        }
    }

    [HttpPatch("{query}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> PatchUser(string query, [FromBody] UpdateUserRequest request)
    {
        var users = await _userRepository.SearchUsersAsync(query);
        var user = users.FirstOrDefault();
        
        if (user == null)
        {
            return NotFound(new { 
                success = false, 
                message = "Usuário não encontrado", 
                errors = new List<string>(),
                data = (UserDto?)null
            });
        }
        
        try
        {
            var updatedUser = await _userService.UpdateUserAsync(user.IdUser, request);

            if (updatedUser == null)
            {
                return NotFound(new { 
                    success = false, 
                    message = "Usuário não encontrado", 
                    errors = new List<string>(),
                    data = (UserDto?)null
                });
            }

            return Ok(new { 
                success = true, 
                message = "Usuário atualizado com sucesso",
                data = updatedUser
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { 
                success = false, 
                message = ex.Message, 
                errors = new List<string>(),
                data = (UserDto?)null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar usuário");
            return BadRequest(new { 
                success = false, 
                message = "Erro ao atualizar usuário", 
                errors = new List<string> { ex.Message },
                data = (UserDto?)null
            });
        }
    }

    [HttpDelete("{query}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteUser(string query)
    {
        var users = await _userRepository.SearchUsersAsync(query);
        var user = users.FirstOrDefault();
        
        if (user == null)
        {
            return NotFound(new { 
                success = false, 
                message = "Usuário não encontrado", 
                errors = new List<string>(),
                data = false
            });
        }
        
        var deleted = await _userService.DeleteUserAsync(user.IdUser);

        if (!deleted)
        {
            return NotFound(new { 
                success = false, 
                message = "Usuário não encontrado", 
                errors = new List<string>(),
                data = false
            });
        }

        return Ok(new { 
            success = true, 
            message = "Usuário deletado com sucesso",
            data = true
        });
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PagedResponse<UserDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAllUsers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var allUsers = await _userService.GetAllUsersAsync();
            var totalRecords = allUsers.Count;
            
            var skip = (pageNumber - 1) * pageSize;
            var data = allUsers.Skip(skip).Take(pageSize).ToList();
            
            var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            var hasPreviousPage = pageNumber > 1;
            var hasNextPage = pageNumber < totalPages;

            return Ok(new { 
                success = true, 
                message = $"{data.Count} usuário(s) encontrado(s) na página {pageNumber}",
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
            _logger.LogError(ex, "Erro ao listar usuários paginados");
            return BadRequest(new { 
                success = false, 
                message = "Erro ao listar usuários paginados", 
                errors = new List<string> { ex.Message },
                data = new List<UserDto>(),
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
    [ProducesResponseType(typeof(PagedResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PagedResponse<UserDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchUsers(
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
                    data = new List<UserDto>(),
                    pageNumber = 0,
                    pageSize = 0,
                    totalRecords = 0,
                    totalPages = 0,
                    hasPreviousPage = false,
                    hasNextPage = false
                });
            }

            var allUsers = await _userService.SearchUsersAsync(query);
            var totalRecords = allUsers.Count;
            
            var skip = (pageNumber - 1) * pageSize;
            var data = allUsers.Skip(skip).Take(pageSize).ToList();
            
            var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            var hasPreviousPage = pageNumber > 1;
            var hasNextPage = pageNumber < totalPages;

            return Ok(new { 
                success = true, 
                message = $"{data.Count} usuário(s) encontrado(s) na busca, página {pageNumber}",
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
            _logger.LogError(ex, "Erro ao buscar usuários paginados");
            return BadRequest(new { 
                success = false, 
                message = "Erro ao buscar usuários paginados", 
                errors = new List<string> { ex.Message },
                data = new List<UserDto>(),
                pageNumber = 0,
                pageSize = 0,
                totalRecords = 0,
                totalPages = 0,
                hasPreviousPage = false,
                hasNextPage = false
            });
        }
    }

    [HttpGet("count")]
    [ProducesResponseType(typeof(ApiResponse<List<UserDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<List<UserDto>>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CountUsers()
    {
        try
        {
            var users = await _userService.GetAllUsersAsync();

            return Ok(new { 
                success = true, 
                message = $"{users.Count} usuário(s) encontrado(s)",
                data = users,
                totalCount = users.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao contar usuários");
            return BadRequest(new { 
                success = false, 
                message = "Erro ao contar usuários", 
                errors = new List<string> { ex.Message },
                data = new List<UserDto>(),
                totalCount = 0
            });
        }
    }
}

