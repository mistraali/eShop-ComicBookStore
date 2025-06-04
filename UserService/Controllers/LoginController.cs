using System.Security.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using UserService.Application.Services;
using UserService.Domain.Models;
using UserService.Domain.Exceptions;
using UserService.Domain.DTOs;
using UserService.Domain.Repositories;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UserService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        protected ILoginService _loginService;
        protected IUserRepository _userRepository;

        public LoginController(ILoginService loginService, IUserRepository userRepository)
        {
            _loginService = loginService;
            _userRepository = userRepository;
        }


        [HttpPost]
        public async Task<IActionResult> Login([FromBody] UserLoginDto loginDto)
        {
            try
            {
                var token = await _loginService.Login(loginDto.Username, loginDto.Password);
                return Ok(new { token });
            }
            catch (InvalidCredentialsException)
            {
                return Unauthorized();
            }
        }
    }
}
