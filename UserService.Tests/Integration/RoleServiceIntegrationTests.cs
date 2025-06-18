using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UserService.Application.Services;
using UserService.Domain.Models;
using UserService.Domain.Repositories;
using Xunit;

namespace UserService.Tests.Integration;
public class RoleServiceIntegrationTests : IDisposable
{
    private readonly UserDataContext _context;
    private readonly RoleService _roleService;
    private readonly IRoleRepository _roleRepository;

    public RoleServiceIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<UserDataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new UserDataContext(options);

        _roleRepository = new RoleRepository(_context);
        _roleService = new RoleService(_roleRepository);

        _context.Roles.Add(new Role { Name = "Admin" });
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task AddRoleAsync_CreatesRole()
    {
        var roleName = "Moderator";
        var role = await _roleService.AddRoleAsync(roleName);
        Assert.NotNull(role);
        Assert.Equal(roleName, role.Name);
        Assert.True(role.Id > 0);
    }

    [Fact]
    public async Task DeleteRoleAsync_DeletesRole()
    {
        var roleName = "Admin";
        var result = await _roleService.DeleteRoleAsync(roleName);
        Assert.True(result);
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
        Assert.Null(role);
    }

    [Fact]
    public async Task DeleteRoleAsync_ReturnsFalseIfRoleNotFound()
    {
        var result = await _roleService.DeleteRoleAsync("NonExistingRole");
        Assert.False(result);
    }

    [Fact]
    public async Task GetAllRolesAsync_ReturnsRoles()
    {
        var roles = await _roleService.GetAllRolesAsync();
        Assert.NotEmpty(roles);
        Assert.Contains(roles, r => r.Name == "Admin");
    }

    [Fact]
    public async Task GetRoleByIdAsync_ReturnsRole()
    {
        var existingRole = await _context.Roles.FirstAsync();
        var role = await _roleService.GetRoleByIdAsync(existingRole.Id);
        Assert.NotNull(role);
        Assert.Equal(existingRole.Name, role.Name);
    }

    [Fact]
    public async Task GetRoleByIdAsync_ReturnsNullForNonExistingId()
    {
        var role = await _roleService.GetRoleByIdAsync(-1);
        Assert.Null(role);
    }

    [Fact]
    public async Task GetRoleByNameAsync_ReturnsRole()
    {
        var existingRole = await _context.Roles.FirstAsync();
        var role = await _roleService.GetRoleByNameAsync(existingRole.Name);
        Assert.NotNull(role);
        Assert.Equal(existingRole.Id, role.Id);
    }

    [Fact]
    public async Task GetRoleByNameAsync_ReturnsNullForNonExistingName()
    {
        var role = await _roleService.GetRoleByNameAsync("NonExistingRole");
        Assert.Null(role);
    }
}

public class RoleRepository : IRoleRepository
{
    private readonly UserDataContext _context;

    public RoleRepository(UserDataContext context)
    {
        _context = context;
    }

    public async Task<Role> AddRoleAsync(Role role)
    {
        _context.Roles.Add(role);
        await _context.SaveChangesAsync();
        return role;
    }

    public async Task DeleteRoleAsync(Role role)
    {
        _context.Roles.Remove(role);
        await _context.SaveChangesAsync();
    }

    public async Task<Role> GetByIdAsync(int id)
    {
        return await _context.Roles.FindAsync(id);
    }

    public async Task<Role> GetByNameAsync(string name)
    {
        return await _context.Roles.FirstOrDefaultAsync(r => r.Name == name);
    }

    public async Task<List<Role>> GetAllRolesAsync()
    {
        return await _context.Roles.ToListAsync();
    }
}
