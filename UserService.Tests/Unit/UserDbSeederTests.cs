using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UserService.Domain.Models;
using UserService.Domain.Seeders;
using UserService.Domain.Repositories;
using Xunit;

namespace UserService.Tests.Unit
{
    public class UserDbSeederTests : IDisposable
    {
        private readonly UserDataContext _context;
        private readonly UserDbSeeder _seeder;

        public UserDbSeederTests()
        {
            var options = new DbContextOptionsBuilder<UserDataContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new UserDataContext(options);
            _seeder = new UserDbSeeder(_context);
        }

        [Fact]
        public async Task Seed_AddsRolesAndUsers_WhenDatabaseIsEmpty()
        {
            await _seeder.Seed();

            var roles = await _context.Roles.ToListAsync();
            var users = await _context.Users.Include(u => u.Roles).ToListAsync();

            Assert.Equal(3, roles.Count);
            Assert.Contains(roles, r => r.Name == "Admin");
            Assert.Contains(roles, r => r.Name == "Employee");
            Assert.Contains(roles, r => r.Name == "Client");

            Assert.Equal(3, users.Count);

            var adminUser = users.FirstOrDefault(u => u.Username == "admin");
            Assert.NotNull(adminUser);
            Assert.True(adminUser.IsActive);
            Assert.Contains(adminUser.Roles, r => r.Name == "Admin");
            Assert.Contains(adminUser.Roles, r => r.Name == "Employee");
            Assert.Contains(adminUser.Roles, r => r.Name == "Client");
        }

        [Fact]
        public async Task Seed_DoesNotAddUsers_WhenUsersAlreadyExist()
        {
            // Arrange: Add a user to simulate existing data
            var role = new Role { Name = "TestRole" };
            _context.Roles.Add(role);
            var existingUser = new User
            {
                Username = "existingUser",
                Email = "exist@example.com",
                Password = "pass",
                Roles = new[] { role }
            };
            _context.Users.Add(existingUser);
            await _context.SaveChangesAsync();

            // Act
            await _seeder.Seed();

            // Assert
            var users = await _context.Users.ToListAsync();
            // There should still be only 1 user (existingUser), seeder did not add the 3 default users
            Assert.Single(users);
            Assert.Contains(users, u => u.Username == "existingUser");
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
