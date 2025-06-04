using Microsoft.AspNetCore.Mvc;
using UserService.Application.Services;
using UserService.Domain.DTOs;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UserService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegisterController : ControllerBase
    {
        private IUserService _userService;
        public RegisterController(IUserService userService)
        {
            _userService = userService;
        }

        // POST api/<UserController>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] UserRegisterDto userRegisterDto)
        {
            var result = await _userService.AddUserAsync(userRegisterDto);

            return Ok(result);
        }
    }
}
