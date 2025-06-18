using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UserService.Application.Services;
using UserService.Domain.DTOs;
using UserService.Domain.Models;
using UserService.Domain.Repositories;
using Xunit;

namespace UserService.Tests.Integration
{
    public class UserServiceIntegrationTests
    {
        private UserDataContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<UserDataContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new UserDataContext(options);
        }

        private IUserService CreateService(UserDataContext context)
        {
            var userRepo = new UserRepository(context);
            var roleRepo = new RoleRepository(context);
            return new UserService.Application.Services.UserService(userRepo, roleRepo);
        }

        private async Task SeedDefaultRole(UserDataContext context)
        {
            var role = new Role { Name = "Client" };
            await context.Roles.AddAsync(role);
            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task AddUserAsync_ValidUser_ReturnsUserReadDto()
        {
            var context = CreateInMemoryContext();
            await SeedDefaultRole(context);
            var service = CreateService(context);

            var dto = new UserRegisterDto
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "password123"
            };

            var result = await service.AddUserAsync(dto);

            Assert.Equal("testuser", result.Username);
            Assert.Contains("Client", result.Roles);
        }

        [Fact]
        public async Task AddUserAsync_MissingRole_ThrowsException()
        {
            var context = CreateInMemoryContext();
            var service = CreateService(context);

            var dto = new UserRegisterDto
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "password123"
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.AddUserAsync(dto));
        }

        [Fact]
        public async Task GetUserByIdAsync_ExistingUser_ReturnsAdminReadDto()
        {
            var context = CreateInMemoryContext();
            await SeedDefaultRole(context);
            var service = CreateService(context);

            var user = new User
            {
                Username = "admin",
                Email = "admin@example.com",
                Password = "pass",
                Roles = new List<Role> { context.Roles.First() }
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var result = await service.GetUserByIdAsync(user.Id);

            Assert.Equal("admin", result.Username);
        }

        [Fact]
        public async Task GetUserByIdAsync_NonExistingUser_ThrowsException()
        {
            var context = CreateInMemoryContext();
            var service = CreateService(context);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetUserByIdAsync(999));
        }

        [Fact]
        public async Task GetUserByUsernameAsync_ExistingUser_ReturnsAdminReadDto()
        {
            var context = CreateInMemoryContext();
            await SeedDefaultRole(context);
            var service = CreateService(context);

            var user = new User
            {
                Username = "admin",
                Email = "admin@example.com",
                Password = "pass",
                Roles = new List<Role> { context.Roles.First() }
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var result = await service.GetUserByUsernameAsync("admin");

            Assert.Equal("admin", result.Username);
        }

        [Fact]
        public async Task GetUserByUsernameAsync_InvalidUsername_ThrowsException()
        {
            var context = CreateInMemoryContext();
            var service = CreateService(context);

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetUserByUsernameAsync("notfound"));
        }

        [Fact]
        public async Task ChangePasswordAsync_ValidData_ChangesPassword()
        {
            var context = CreateInMemoryContext();
            await SeedDefaultRole(context);
            var service = CreateService(context);

            var user = new User
            {
                Username = "user1",
                Email = "user1@example.com",
                Password = "oldpass",
                Roles = new List<Role> { context.Roles.First() }
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var dto = new UserChangePasswordDto
            {
                OldPassword = "oldpass",
                NewPassword = "newpass"
            };

            var result = await service.ChangePasswordAsync(user.Id, dto);

            Assert.True(result);
        }

        [Fact]
        public async Task ChangePasswordAsync_WrongOldPassword_ThrowsException()
        {
            var context = CreateInMemoryContext();
            await SeedDefaultRole(context);
            var service = CreateService(context);

            var user = new User
            {
                Username = "user1",
                Email = "user1@example.com",
                Password = "oldpass",
                Roles = new List<Role> { context.Roles.First() }
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var dto = new UserChangePasswordDto
            {
                OldPassword = "wrongpass",
                NewPassword = "newpass"
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.ChangePasswordAsync(user.Id, dto));
        }

        [Fact]
        public async Task EditUserAccountAsync_ValidData_UpdatesUser()
        {
            var context = CreateInMemoryContext();
            await SeedDefaultRole(context);
            var service = CreateService(context);

            var user = new User
            {
                Username = "user1",
                Email = "user1@example.com",
                Password = "pass",
                Roles = new List<Role> { context.Roles.First() }
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var dto = new UserEditDto
            {
                NewUsername = "newname",
                NewEmail = "new@example.com"
            };

            var result = await service.EditUserAccountAsync(user.Id, dto);

            Assert.True(result);
        }

        [Fact]
        public async Task EditUserAccountAsync_InvalidUser_ThrowsException()
        {
            var context = CreateInMemoryContext();
            var service = CreateService(context);

            var dto = new UserEditDto
            {
                NewUsername = "newname",
                NewEmail = "new@example.com"
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.EditUserAccountAsync(999, dto));
        }

        [Fact]
        public async Task AdminEditUserAccountAsync_ValidData_UpdatesUser()
        {
            var context = CreateInMemoryContext();
            await SeedDefaultRole(context);
            var service = CreateService(context);

            var user = new User
            {
                Username = "admin",
                Email = "admin@example.com",
                Password = "oldpass",
                Roles = new List<Role> { context.Roles.First() }
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var dto = new UserUpdateDto
            {
                NewUsername = "admin2",
                NewEmail = "admin2@example.com",
                NewPassword = "newpass123",
                NewIsActive = false
            };

            var result = await service.AdminEditUserAccountAsync(user.Id, dto);

            Assert.True(result);
        }

        [Fact]
        public async Task AdminEditUserAccountAsync_InvalidUser_ThrowsException()
        {
            var context = CreateInMemoryContext();
            var service = CreateService(context);

            var dto = new UserUpdateDto
            {
                NewUsername = "admin2"
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.AdminEditUserAccountAsync(999, dto));
        }

        [Fact]
        public async Task AdminEditUserAccountAsync_ShortPassword_ThrowsException()
        {
            var context = CreateInMemoryContext();
            await SeedDefaultRole(context);
            var service = CreateService(context);

            var user = new User
            {
                Username = "admin",
                Email = "admin@example.com",
                Password = "oldpass",
                Roles = new List<Role> { context.Roles.First() }
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var dto = new UserUpdateDto
            {
                NewPassword = "123"
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.AdminEditUserAccountAsync(user.Id, dto));
        }
    }
}
