using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.Application.Services;
using UserService.Domain.DTOs;
using UserService.Domain.Models;
using UserService.Domain.Repositories;
using UserService.Controllers;
using Xunit;

namespace UserService.Tests.Integration;
public class UserPanelControllerTests : IDisposable
{
    private readonly UserDataContext _context;
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IUserService _userService;
    private readonly UserPanelController _controller;

    public UserPanelControllerTests()
    {
        var options = new DbContextOptionsBuilder<UserDataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new UserDataContext(options);

        SeedDatabase();

        _userRepository = new UserRepository(_context);
        _roleRepository = new RoleRepository(_context);
        _userService = new UserService.Application.Services.UserService(_userRepository, _roleRepository);
        _controller = new UserPanelController(_userService);
    }

    private void SeedDatabase()
    {
        var roleClient = new Role { Name = "Client" };
        _context.Roles.Add(roleClient);

        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            Password = "oldpassword",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            Roles = { roleClient }
        };
        var user1 = new User
        {
            Id = 5,
            Username = "testuser",
            Email = "test@example.com",
            Password = "oldpassword",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            Roles = { roleClient }
        };
        _context.Users.Add(user);
        _context.Users.Add(user1);
        _context.SaveChanges();
    }

    private void AuthenticateController(int userId)
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        }, "TestAuth"));

        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = user }
        };
    }

    private void UnauthenticateController()
    {
        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = new ClaimsPrincipal() }
        };
    }

    [Fact]
    public async Task Get_ReturnsOk_WithUserDto_WhenAuthenticated()
    {
        AuthenticateController(1);

        var result = await _controller.Get();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var userDto = Assert.IsType<UserReadDto>(okResult.Value);
        Assert.Equal("testuser", userDto.Username);
        Assert.Contains("Client", userDto.Roles);
    }

    [Fact]
    public async Task Get_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        UnauthenticateController();

        var result = await _controller.Get();

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task ChangePassword_ReturnsOk_WhenPasswordChanged()
    {
        AuthenticateController(1);

        var dto = new UserChangePasswordDto
        {
            OldPassword = "oldpassword",
            NewPassword = "newpassword"
        };

        var result = await _controller.ChangePassword(dto);

        var okResult = Assert.IsAssignableFrom<ObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Contains("Password has been changed succesfully", okResult.Value.ToString());

        var user = await _userRepository.GetUserByIdAsync(1);
        Assert.Equal("newpassword", user.Password);
    }

    [Fact]
    public async Task ChangePassword_ReturnsBadRequest_WhenOldPasswordIncorrect()
    {
        AuthenticateController(1);

        var dto = new UserChangePasswordDto
        {
            OldPassword = "wrongpassword",
            NewPassword = "newpassword"
        };

        var result = await _controller.ChangePassword(dto);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, objectResult.StatusCode);
        Assert.Contains("Impossible to change password. Verify your current password.", objectResult.Value.ToString());

        var user = await _userRepository.GetUserByIdAsync(1);
        Assert.Equal("oldpassword", user.Password);
    }


    [Fact]
    public async Task ChangePassword_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        UnauthenticateController();

        var dto = new UserChangePasswordDto
        {
            OldPassword = "oldpassword",
            NewPassword = "newpassword"
        };

        var result = await _controller.ChangePassword(dto);

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task EditAccount_ReturnsOk_WhenAccountModified()
    {
        AuthenticateController(1);

        var dto = new UserEditDto
        {
            NewUsername = "newusername",
            NewEmail = "newemail@example.com"
        };

        var result = await _controller.EditAccount(dto);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Contains("Account has been modified succesfully", okResult.Value.ToString());

        var user = await _userRepository.GetUserByIdAsync(1);
        Assert.Equal("newusername", user.Username);
        Assert.Equal("newemail@example.com", user.Email);
    }

    [Fact]
    public async Task EditAccount_ReturnsBadRequest_WhenUserDoesNotExist()
    {
        AuthenticateController(999); // non-existing user

        var dto = new UserEditDto
        {
            NewUsername = "newusername",
            NewEmail = "newemail@example.com"
        };

        var result = await _controller.EditAccount(dto);

        var badRequestResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, badRequestResult.StatusCode);
        Assert.Contains("User with id: 999 does not exist", badRequestResult.Value.ToString());
    }

    [Fact]
    public async Task EditAccount_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        UnauthenticateController();

        var dto = new UserEditDto
        {
            NewUsername = "newusername",
            NewEmail = "newemail@example.com"
        };

        var result = await _controller.EditAccount(dto);

        Assert.IsType<UnauthorizedResult>(result);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}