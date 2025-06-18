using Microsoft.AspNetCore.Mvc;
using UserService.Domain.DTOs;
using UserService.Application.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using UserService.Domain.Exceptions;
using System.Linq.Expressions;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UserService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserPanelController : ControllerBase
{
    
    private IUserService _userService;
    public UserPanelController(IUserService userService)
    {
        _userService = userService;
    }

    // GET api/<UserController>/5
    [HttpGet("get-current-user")]
    [Authorize(Policy = "RegisteredOnly")]
    public async Task<IActionResult> Get()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return Unauthorized();

        var userId = int.Parse(userIdClaim.Value);
        var userDto = await _userService.GetUserDtoByIdAsync(userId);

        return Ok(userDto);

    }

    // PATCH api/<UserController>/5
    [HttpPatch("change-password")]
    [Authorize(Policy = "RegisteredOnly")]
    public async Task<IActionResult> ChangePassword([FromBody] UserChangePasswordDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return Unauthorized();

        var userId = int.Parse(userIdClaim.Value);

        try
        {
            var result = await _userService.ChangePasswordAsync(userId, dto);

            if (result)
                return Ok(new { message = "Password has been changed succesfully." });

            throw new InvalidOperationException();
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(400, new { message = "Impossible to change password. Verify your current password." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Server error.", error = ex.Message });
        }

    }

    // PUT api/<UserController>/5
    [HttpPut("edit-account")]
    [Authorize(Policy = "RegisteredOnly")]
    public async Task<IActionResult> EditAccount([FromBody] UserEditDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return Unauthorized();

        var userId = int.Parse(userIdClaim.Value);

        try
        {
            var result = await _userService.EditUserAccountAsync(userId, dto);

            if (result)
                return Ok(new { message = "Account has been modified succesfully." });

            return BadRequest(new { message = "Impossible to modify account. Contact your administrator." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Server error.", error = ex.Message });
        }

    }

}