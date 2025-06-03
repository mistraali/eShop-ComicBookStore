using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.Services;
using UserService.Domain.DTOs;
using UserService.Domain.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UserService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AdminPanelController : ControllerBase
{
    private IUserService _userService;
    public AdminPanelController(IUserService userService)
    {
        _userService = userService;
    }

    // GET: api/<UserController>
    [HttpGet]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Get()
    {
        var result = await _userService.GetAllUsersAsync();

        return Ok(result);
    }

    // GET api/<UserController>/5
    [HttpGet("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Get(int id)
    {
        var result = await _userService.GetUserByIdAsync(id);

        return Ok(result);
    }

    // PUT api/<UserController>/5
    [HttpPut("edit-user-account/{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> AdminEditUserAccountAsync(int userId, [FromBody] UserUpdateDto dto)
    {
        try
        {
            var result = await _userService.AdminEditUserAccountAsync(userId, dto);

            if (result)
                return Ok(new { message = "Konto zmodyfikowane pomyślnie." });

            return BadRequest(new { message = "Nie udało się wyedytować konta." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Wystąpił błąd serwera.", error = ex.Message });
        }
    }
}
