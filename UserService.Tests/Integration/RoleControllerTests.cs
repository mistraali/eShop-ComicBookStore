using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.Application.Services;
using UserService.Controllers;
using UserService.Domain.Models;
using UserService.Domain.Repositories;
using UserService.Domain.Seeders;
using Xunit;

namespace UserService.Tests.Integration
{
    public class RoleControllerTests
    {
        private readonly RoleController _controller;
        private readonly UserDataContext _context;
        private readonly RoleService _roleService;
        private readonly RoleRepository _roleRepository;

        public RoleControllerTests()
        {
            var options = new DbContextOptionsBuilder<UserDataContext>()
                .UseInMemoryDatabase(databaseName: "RoleControllerTestsDb")
                .Options;

            _context = new UserDataContext(options);

            var seeder = new UserDbSeeder(_context);
            seeder.Seed().Wait();

            _roleRepository = new RoleRepository(_context);
            _roleService = new RoleService(_roleRepository);
            _controller = new RoleController(_roleService);
        }

        [Fact]
        public async Task GetAllRoles_ReturnsOkAndRoles()
        {
            var result = await _controller.Get();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var roles = Assert.IsAssignableFrom<List<Role>>(okResult.Value);
            Assert.NotEmpty(roles);
        }

        [Fact]
        public async Task GetRoleById_ReturnsOk_WhenRoleExists()
        {
            var adminRole = await _roleRepository.GetByNameAsync("Admin");
            var result = await _controller.Get(adminRole.Id);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var role = Assert.IsType<Role>(okResult.Value);
            Assert.Equal("Admin", role.Name);
        }

        [Fact]
        public async Task GetRoleById_ReturnsNotFound_WhenRoleDoesNotExist()
        {
            var result = await _controller.Get(9999);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains("nie została znaleziona", notFoundResult.Value.ToString());
        }

        [Fact]
        public async Task GetRoleByName_ReturnsOk_WhenRoleExists()
        {
            var result = await _controller.Get("Employee");

            var okResult = Assert.IsType<OkObjectResult>(result);
            var role = Assert.IsType<Role>(okResult.Value);
            Assert.Equal("Employee", role.Name);
        }

        [Fact]
        public async Task GetRoleByName_ReturnsNotFound_WhenRoleDoesNotExist()
        {
            var result = await _controller.Get("NoRole");

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains("nie została znaleziona", notFoundResult.Value.ToString());
        }

        [Fact]
        public async Task Post_AddsNewRole_ReturnsOk()
        {
            var roleName = "NewRole";

            var result = await _controller.Post(roleName);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var role = Assert.IsType<Role>(okResult.Value);
            Assert.Equal(roleName, role.Name);
        }

        [Fact]
        public async Task Delete_RemovesRole_ReturnsOk()
        {
            var roleName = "Client";

            var result = await _controller.Delete(roleName);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("została usunięta", okResult.Value.ToString());
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenRoleDoesNotExist()
        {
            var result = await _controller.Delete("NonExistentRole");

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains("nie została znaleziona", notFoundResult.Value.ToString());
        }
    }
}
