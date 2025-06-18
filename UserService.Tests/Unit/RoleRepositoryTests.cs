using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using UserService.Domain.Models;
using UserService.Domain.Repositories;
using Xunit;

namespace UserService.Tests.Unit
{
    public class RoleRepositoryTests : IDisposable
    {
        private readonly UserDataContext _context;
        private readonly RoleRepository _repository;

        public RoleRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<UserDataContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new UserDataContext(options);
            _repository = new RoleRepository(_context);

            SeedData().Wait();
        }

        private async Task SeedData()
        {
            var roles = new List<Role>
            {
                new Role { Id = 1, Name = "Admin" },
                new Role { Id = 2, Name = "User" },
                new Role { Id = 3, Name = "Guest" }
            };

            await _context.Roles.AddRangeAsync(roles);
            await _context.SaveChangesAsync();
        }

        [Fact]
        public async Task GetAllRolesAsync_ReturnsAllRoles()
        {
            var roles = await _repository.GetAllRolesAsync();

            Assert.NotNull(roles);
            Assert.Equal(3, roles.Count);
        }

        [Theory]
        [InlineData("Admin", 1)]
        [InlineData("User", 2)]
        [InlineData("Guest", 3)]
        public async Task GetByNameAsync_ExistingRole_ReturnsRole(string roleName, int expectedId)
        {
            var role = await _repository.GetByNameAsync(roleName);

            Assert.NotNull(role);
            Assert.Equal(expectedId, role.Id);
            Assert.Equal(roleName, role.Name);
        }

        [Fact]
        public async Task GetByNameAsync_NonExistingRole_ReturnsNull()
        {
            var role = await _repository.GetByNameAsync("NonExistingRole");

            Assert.Null(role);
        }

        [Theory]
        [InlineData(1, "Admin")]
        [InlineData(2, "User")]
        [InlineData(3, "Guest")]
        public async Task GetByIdAsync_ExistingRole_ReturnsRole(int id, string expectedName)
        {
            var role = await _repository.GetByIdAsync(id);

            Assert.NotNull(role);
            Assert.Equal(id, role.Id);
            Assert.Equal(expectedName, role.Name);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistingRole_ReturnsNull()
        {
            var role = await _repository.GetByIdAsync(999);

            Assert.Null(role);
        }

        [Fact]
        public async Task AddRoleAsync_AddsRoleSuccessfully()
        {
            var newRole = new Role { Name = "Moderator" };

            var addedRole = await _repository.AddRoleAsync(newRole);

            Assert.NotNull(addedRole);
            Assert.True(addedRole.Id > 0);

            var fromDb = await _repository.GetByNameAsync("Moderator");
            Assert.NotNull(fromDb);
            Assert.Equal("Moderator", fromDb.Name);
        }

        [Fact]
        public async Task DeleteRoleAsync_DeletesRoleSuccessfully()
        {
            var role = await _repository.GetByNameAsync("Guest");
            Assert.NotNull(role);

            await _repository.DeleteRoleAsync(role);

            var deletedRole = await _repository.GetByNameAsync("Guest");
            Assert.Null(deletedRole);
        }

        [Fact]
        public async Task DeleteRoleAsync_DeletingNonExistingRole_DoesNotThrow()
        {
            var nonExistingRole = new Role { Id = 999, Name = "NonExistingRole" };

            var exception = await Record.ExceptionAsync(() => _repository.DeleteRoleAsync(nonExistingRole));

            Assert.Null(exception);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
