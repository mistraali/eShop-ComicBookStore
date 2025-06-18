using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;
using UserService.Application.Services;
using UserService.Domain.DTOs;
using UserService.Domain.Models;
using UserService.Domain.Repositories;

namespace UserService.Tests.Unit;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IRoleRepository> _roleRepoMock = new();
    private readonly UserService.Application.Services.UserService _sut;

    public UserServiceTests()
    {
        _sut = new UserService.Application.Services.UserService(_userRepoMock.Object, _roleRepoMock.Object);
    }

    [Fact]
    public async Task AddUserAsync_DefaultRoleExists_AddsUserAndReturnsDto()
    {
        // Arrange
        var clientRole = new Role { Id = 1, Name = "Client" };
        _roleRepoMock.Setup(r => r.GetByNameAsync("Client"))
            .ReturnsAsync(clientRole);

        var userToAdd = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "password",
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            Roles = new List<Role> { clientRole }
        };

        var userResult = new User
        {
            Id = 10,
            Username = "testuser",
            Email = "test@example.com",
            CreatedAt = DateTime.UtcNow,
            Roles = new List<Role> { clientRole }
        };

        _userRepoMock.Setup(r => r.AddUserAsync(It.Is<User>(u => u.Username == userToAdd.Username)))
            .ReturnsAsync(userResult);

        var dto = new UserRegisterDto
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "password"
        };

        // Act
        var result = await _sut.AddUserAsync(dto);

        // Assert
        Assert.Equal(userResult.Id, result.Id);
        Assert.Equal(userResult.Username, result.Username);
        Assert.Equal(userResult.Email, result.Email);
        Assert.Contains("Client", result.Roles);
    }

    [Fact]
    public async Task AddUserAsync_NoDefaultRole_ThrowsInvalidOperationException()
    {
        _roleRepoMock.Setup(r => r.GetByNameAsync("Client")).ReturnsAsync((Role)null);

        var dto = new UserRegisterDto
        {
            Username = "test",
            Email = "test@test.com",
            Password = "pass"
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.AddUserAsync(dto));
    }

    [Fact]
    public async Task GetAllUsersAsync_ReturnsListOfAdminReadDto()
    {
        var users = new List<User>
        {
            new User
            {
                Id = 1,
                Username = "user1",
                Email = "u1@test.com",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow,
                Roles = new List<Role> { new Role { Name = "Client" } },
                Password = "pass1"
            },
            new User
            {
                Id = 2,
                Username = "user2",
                Email = "u2@test.com",
                IsActive = false,
                CreatedAt = DateTime.UtcNow,
                Roles = new List<Role> { new Role { Name = "Admin" } },
                Password = "pass2"
            }
        };
        _userRepoMock.Setup(r => r.GetAllUsersAsync()).ReturnsAsync(users);

        var result = await _sut.GetAllUsersAsync();

        Assert.Equal(users.Count, result.Count);
        Assert.Contains(result, r => r.Username == "user1" && r.Roles.Contains("Client"));
        Assert.Contains(result, r => r.Username == "user2" && r.Roles.Contains("Admin"));
    }

    [Fact]
    public async Task GetUserByIdAsync_UserExists_ReturnsAdminReadDto()
    {
        var user = new User
        {
            Id = 1,
            Username = "user1",
            Email = "u1@test.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow,
            Roles = new List<Role> { new Role { Name = "Client" } },
            Password = "pass1"
        };

        _userRepoMock.Setup(r => r.GetUserByIdAsync(1)).ReturnsAsync(user);

        var result = await _sut.GetUserByIdAsync(1);

        Assert.Equal(user.Id, result.Id);
        Assert.Equal(user.Username, result.Username);
        Assert.Contains("Client", result.Roles);
    }

    [Fact]
    public async Task GetUserByIdAsync_UserDoesNotExist_ThrowsInvalidOperationException()
    {
        _userRepoMock.Setup(r => r.GetUserByIdAsync(99)).ReturnsAsync((User)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.GetUserByIdAsync(99));
    }

    [Fact]
    public async Task GetUserDtoByIdAsync_UserExists_ReturnsUserReadDto()
    {
        var user = new User
        {
            Id = 1,
            Username = "user1",
            Email = "u1@test.com",
            CreatedAt = DateTime.UtcNow,
            Roles = new List<Role> { new Role { Name = "Client" } }
        };
        _userRepoMock.Setup(r => r.GetUserByIdAsync(1)).ReturnsAsync(user);

        var result = await _sut.GetUserDtoByIdAsync(1);

        Assert.Equal(user.Id, result.Id);
        Assert.Equal(user.Username, result.Username);
        Assert.Contains("Client", result.Roles);
    }

    [Fact]
    public async Task GetUserDtoByIdAsync_UserDoesNotExist_ThrowsInvalidOperationException()
    {
        _userRepoMock.Setup(r => r.GetUserByIdAsync(99)).ReturnsAsync((User)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.GetUserDtoByIdAsync(99));
    }

    [Fact]
    public async Task GetUserByUsernameAsync_UserExists_ReturnsAdminReadDto()
    {
        var user = new User
        {
            Id = 5,
            Username = "testuser",
            Email = "test@example.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            Roles = new List<Role> { new Role { Name = "Client" } },
            Password = "pass123"
        };

        _userRepoMock.Setup(r => r.GetUserByUsernameAsync("testuser")).ReturnsAsync(user);

        var result = await _sut.GetUserByUsernameAsync("testuser");

        Assert.Equal(user.Id, result.Id);
        Assert.Equal(user.Username, result.Username);
        Assert.Contains("Client", result.Roles);
    }

    [Fact]
    public async Task GetUserByUsernameAsync_UserDoesNotExist_ThrowsInvalidOperationException()
    {
        _userRepoMock.Setup(r => r.GetUserByUsernameAsync("nouser")).ReturnsAsync((User)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.GetUserByUsernameAsync("nouser"));
    }

    [Fact]
    public async Task ChangePasswordAsync_OldPasswordMatches_UpdatesPasswordAndReturnsTrue()
    {
        var user = new User
        {
            Id = 1,
            Password = "oldpass"
        };

        _userRepoMock.Setup(r => r.GetUserByIdAsync(1)).ReturnsAsync(user);
        _userRepoMock.Setup(r => r.UpdateUserAsync(It.IsAny<User>())).ReturnsAsync((User u) => u);

        var dto = new UserChangePasswordDto
        {
            OldPassword = "oldpass",
            NewPassword = "newpass"
        };

        var result = await _sut.ChangePasswordAsync(1, dto);

        Assert.True(result);
        Assert.Equal("newpass", user.Password);
        _userRepoMock.Verify(r => r.UpdateUserAsync(user), Times.Once);
    }

    [Fact]
    public async Task ChangePasswordAsync_OldPasswordDoesNotMatch_ThrowsInvalidOperationException()
    {
        var user = new User { Id = 1, Password = "oldpass" };
        _userRepoMock.Setup(r => r.GetUserByIdAsync(1)).ReturnsAsync(user);

        var dto = new UserChangePasswordDto
        {
            OldPassword = "wrongold",
            NewPassword = "newpass"
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.ChangePasswordAsync(1, dto));
    }

    [Fact]
    public async Task EditUserAccountAsync_UserExists_UpdatesUsernameAndEmailAndReturnsTrue()
    {
        var user = new User { Id = 1, Username = "olduser", Email = "old@email.com" };
        _userRepoMock.Setup(r => r.GetUserByIdAsync(1)).ReturnsAsync(user);
        _userRepoMock.Setup(r => r.UpdateUserAsync(user)).ReturnsAsync(user);

        var dto = new UserEditDto
        {
            NewUsername = "newuser",
            NewEmail = "new@email.com"
        };

        var result = await _sut.EditUserAccountAsync(1, dto);

        Assert.True(result);
        Assert.Equal("newuser", user.Username);
        Assert.Equal("new@email.com", user.Email);
        _userRepoMock.Verify(r => r.UpdateUserAsync(user), Times.Once);
    }

    [Fact]
    public async Task EditUserAccountAsync_UserDoesNotExist_ThrowsInvalidOperationException()
    {
        _userRepoMock.Setup(r => r.GetUserByIdAsync(99)).ReturnsAsync((User)null);

        var dto = new UserEditDto
        {
            NewUsername = "newuser",
            NewEmail = "new@email.com"
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.EditUserAccountAsync(99, dto));
    }

    [Fact]
    public async Task AdminEditUserAccountAsync_UserExists_ValidUpdates_ReturnsTrue()
    {
        var user = new User
        {
            Id = 1,
            Username = "olduser",
            Email = "old@email.com",
            Password = "oldpass",
            IsActive = true
        };
        _userRepoMock.Setup(r => r.GetUserByIdAsync(1)).ReturnsAsync(user);
        _userRepoMock.Setup(r => r.UpdateUserAsync(user)).ReturnsAsync(user);

        var dto = new UserUpdateDto
        {
            NewUsername = "newuser",
            NewEmail = "new@email.com",
            NewPassword = "newpass",
            NewIsActive = false
        };

        var result = await _sut.AdminEditUserAccountAsync(1, dto);

        Assert.True(result);
        Assert.Equal("newuser", user.Username);
        Assert.Equal("new@email.com", user.Email);
        Assert.Equal("newpass", user.Password);
        Assert.False(user.IsActive);
        _userRepoMock.Verify(r => r.UpdateUserAsync(user), Times.Once);
    }

    [Fact]
    public async Task AdminEditUserAccountAsync_UserDoesNotExist_ThrowsInvalidOperationException()
    {
        _userRepoMock.Setup(r => r.GetUserByIdAsync(99)).ReturnsAsync((User)null);

        var dto = new UserUpdateDto
        {
            NewUsername = "user",
            NewEmail = "email@test.com",
            NewPassword = "password",
            NewIsActive = true
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.AdminEditUserAccountAsync(99, dto));
    }

    [Fact]
    public async Task AdminEditUserAccountAsync_NewPasswordTooShort_ThrowsInvalidOperationException()
    {
        var user = new User
        {
            Id = 1,
            Password = "oldpass"
        };
        _userRepoMock.Setup(r => r.GetUserByIdAsync(1)).ReturnsAsync(user);

        var dto = new UserUpdateDto
        {
            NewPassword = "123"
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.AdminEditUserAccountAsync(1, dto));
    }
}
