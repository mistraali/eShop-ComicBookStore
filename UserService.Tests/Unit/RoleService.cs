using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using UserService.Application.Services;
using UserService.Domain.Models;
using UserService.Domain.Repositories;
using Xunit;

namespace UserService.Tests.Unit;

public class RoleServiceTests
{
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly RoleService _roleService;

    public RoleServiceTests()
    {
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _roleService = new RoleService(_roleRepositoryMock.Object);
    }

    [Fact]
    public async Task AddRoleAsync_ShouldAddRoleAndReturnRole()
    {
        // Arrange
        var roleName = "NewRole";
        var role = new Role { Name = roleName };
        _roleRepositoryMock.Setup(r => r.AddRoleAsync(It.IsAny<Role>())).ReturnsAsync(role);

        // Act
        var result = await _roleService.AddRoleAsync(roleName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(roleName, result.Name);
        _roleRepositoryMock.Verify(r => r.AddRoleAsync(It.Is<Role>(role => role.Name == roleName)), Times.Once);
    }

    [Fact]
    public async Task DeleteRoleAsync_ShouldReturnTrue_WhenRoleExists()
    {
        // Arrange
        var roleName = "RoleToDelete";
        var role = new Role { Name = roleName };
        _roleRepositoryMock.Setup(r => r.GetByNameAsync(roleName)).ReturnsAsync(role);
        _roleRepositoryMock.Setup(r => r.DeleteRoleAsync(role)).Returns(Task.CompletedTask);

        // Act
        var result = await _roleService.DeleteRoleAsync(roleName);

        // Assert
        Assert.True(result);
        _roleRepositoryMock.Verify(r => r.GetByNameAsync(roleName), Times.Once);
        _roleRepositoryMock.Verify(r => r.DeleteRoleAsync(role), Times.Once);
    }

    [Fact]
    public async Task DeleteRoleAsync_ShouldReturnFalse_WhenRoleDoesNotExist()
    {
        // Arrange
        var roleName = "NonExistingRole";
        _roleRepositoryMock.Setup(r => r.GetByNameAsync(roleName)).ReturnsAsync((Role)null);

        // Act
        var result = await _roleService.DeleteRoleAsync(roleName);

        // Assert
        Assert.False(result);
        _roleRepositoryMock.Verify(r => r.GetByNameAsync(roleName), Times.Once);
        _roleRepositoryMock.Verify(r => r.DeleteRoleAsync(It.IsAny<Role>()), Times.Never);
    }

    [Fact]
    public async Task GetAllRolesAsync_ShouldReturnListOfRoles()
    {
        // Arrange
        var roles = new List<Role>
        {
            new Role { Name = "Role1" },
            new Role { Name = "Role2" }
        };
        _roleRepositoryMock.Setup(r => r.GetAllRolesAsync()).ReturnsAsync(roles);

        // Act
        var result = await _roleService.GetAllRolesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        _roleRepositoryMock.Verify(r => r.GetAllRolesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetRoleByIdAsync_ShouldReturnRole_WhenRoleExists()
    {
        // Arrange
        var roleId = 1;
        var role = new Role { Id = roleId, Name = "RoleById" };
        _roleRepositoryMock.Setup(r => r.GetByIdAsync(roleId)).ReturnsAsync(role);

        // Act
        var result = await _roleService.GetRoleByIdAsync(roleId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(roleId, result.Id);
        _roleRepositoryMock.Verify(r => r.GetByIdAsync(roleId), Times.Once);
    }

    [Fact]
    public async Task GetRoleByIdAsync_ShouldReturnNull_WhenRoleDoesNotExist()
    {
        // Arrange
        var roleId = 999;
        _roleRepositoryMock.Setup(r => r.GetByIdAsync(roleId)).ReturnsAsync((Role)null);

        // Act
        var result = await _roleService.GetRoleByIdAsync(roleId);

        // Assert
        Assert.Null(result);
        _roleRepositoryMock.Verify(r => r.GetByIdAsync(roleId), Times.Once);
    }

    [Fact]
    public async Task GetRoleByNameAsync_ShouldReturnRole_WhenRoleExists()
    {
        // Arrange
        var roleName = "RoleByName";
        var role = new Role { Name = roleName };
        _roleRepositoryMock.Setup(r => r.GetByNameAsync(roleName)).ReturnsAsync(role);

        // Act
        var result = await _roleService.GetRoleByNameAsync(roleName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(roleName, result.Name);
        _roleRepositoryMock.Verify(r => r.GetByNameAsync(roleName), Times.Once);
    }

    [Fact]
    public async Task GetRoleByNameAsync_ShouldReturnNull_WhenRoleDoesNotExist()
    {
        // Arrange
        var roleName = "NonExistingRole";
        _roleRepositoryMock.Setup(r => r.GetByNameAsync(roleName)).ReturnsAsync((Role)null);

        // Act
        var result = await _roleService.GetRoleByNameAsync(roleName);

        // Assert
        Assert.Null(result);
        _roleRepositoryMock.Verify(r => r.GetByNameAsync(roleName), Times.Once);
    }
}
