using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UserService.Domain.Models;
using UserService.Domain.Repositories;
using Xunit;

namespace UserService.Tests.Unit
{
    public class UserRepositoryTests : IDisposable
    {
        private readonly UserDataContext _context;
        private readonly UserRepository _repository;

        public UserRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<UserDataContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new UserDataContext(options);
            _repository = new UserRepository(_context);

            SeedData().Wait();
        }

        private async Task SeedData()
        {
            var adminRole = new Role { Id = 1, Name = "Admin" };
            var userRole = new Role { Id = 2, Name = "User" };

            await _context.Roles.AddRangeAsync(adminRole, userRole);

            var users = new List<User>
            {
                new User
                {
                    Id = 1,
                    Username = "admin",
                    Email = "admin@example.com",
                    Password = "hashedpassword",
                    Roles = new List<Role> { adminRole }
                },
                new User
                {
                    Id = 2,
                    Username = "user1",
                    Email = "user1@example.com",
                    Password = "hashedpassword",
                    Roles = new List<Role> { userRole }
                }
            };

            await _context.Users.AddRangeAsync(users);
            await _context.SaveChangesAsync();
        }

        [Fact]
        public async Task GetAllUsersAsync_ReturnsAllUsersWithRoles()
        {
            var users = await _repository.GetAllUsersAsync();

            Assert.NotNull(users);
            Assert.Equal(2, users.Count);

            Assert.All(users, user => Assert.NotEmpty(user.Roles));
        }

        [Fact]
        public async Task GetUserByIdAsync_ExistingUser_ReturnsUserWithRoles()
        {
            var user = await _repository.GetUserByIdAsync(1);

            Assert.NotNull(user);
            Assert.Equal("admin", user.Username);
            Assert.NotEmpty(user.Roles);
            Assert.Contains(user.Roles, r => r.Name == "Admin");
        }

        [Fact]
        public async Task GetUserByIdAsync_NonExistingUser_ReturnsNull()
        {
            var user = await _repository.GetUserByIdAsync(999);

            Assert.Null(user);
        }

        [Fact]
        public async Task GetUserByUsernameAsync_ExistingUser_ReturnsUserWithRoles()
        {
            var user = await _repository.GetUserByUsernameAsync("user1");

            Assert.NotNull(user);
            Assert.Equal(2, user.Id);
            Assert.NotEmpty(user.Roles);
            Assert.Contains(user.Roles, r => r.Name == "User");
        }

        [Fact]
        public async Task GetUserByUsernameAsync_NonExistingUser_ReturnsNull()
        {
            var user = await _repository.GetUserByUsernameAsync("nonexistent");

            Assert.Null(user);
        }

        [Fact]
        public async Task AddUserAsync_AddsUserWithRoles()
        {
            var newRole = new Role { Id = 3, Name = "Guest" };
            await _context.Roles.AddAsync(newRole);
            await _context.SaveChangesAsync();

            var newUser = new User
            {
                Username = "guestuser",
                Email = "guest@example.com",
                Password = "guestpass",
                Roles = new List<Role> { newRole }
            };

            var addedUser = await _repository.AddUserAsync(newUser);

            Assert.NotNull(addedUser);
            Assert.True(addedUser.Id > 0);
            Assert.Equal("guestuser", addedUser.Username);
            Assert.Single(addedUser.Roles);
            Assert.Contains(addedUser.Roles, r => r.Name == "Guest");

            var fromDb = await _repository.GetUserByUsernameAsync("guestuser");
            Assert.NotNull(fromDb);
            Assert.Equal("guestuser", fromDb.Username);
            Assert.NotEmpty(fromDb.Roles);
        }

        [Fact]
        public async Task UpdateUserAsync_UpdatesUserSuccessfully()
        {
            var user = await _repository.GetUserByUsernameAsync("user1");
            Assert.NotNull(user);

            user.Email = "newemail@example.com";
            user.IsActive = false;

            var updatedUser = await _repository.UpdateUserAsync(user);

            Assert.NotNull(updatedUser);
            Assert.Equal("newemail@example.com", updatedUser.Email);
            Assert.False(updatedUser.IsActive);

            var fromDb = await _repository.GetUserByUsernameAsync("user1");
            Assert.Equal("newemail@example.com", fromDb.Email);
            Assert.False(fromDb.IsActive);
        }

        [Fact]
        public async Task UpdateUserAsync_NonExistingUser_AddsUser()
        {
            // EF Core Update with non-tracked entity behaves like Add if entity has key set to 0.
            var newUser = new User
            {
                Id = 999,
                Username = "newuser",
                Email = "newuser@example.com",
                Password = "password",
                Roles = new List<Role>()
            };

            var updatedUser = await _repository.UpdateUserAsync(newUser);

            Assert.NotNull(updatedUser);
            Assert.Equal("newuser", updatedUser.Username);

            var fromDb = await _repository.GetUserByUsernameAsync("newuser");
            Assert.NotNull(fromDb);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
