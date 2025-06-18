using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using UserService.Application.Kafka;
using UserService.Application.Services;
using UserService.Domain.Events;
using UserService.Domain.Exceptions;
using UserService.Domain.JWT;
using UserService.Domain.Models;
using UserService.Domain.Repositories;
using Xunit;

namespace UserService.Tests.Integration
{
    public class LoginServiceIntegrationTests
    {
        private class InMemoryUserRepository : IUserRepository
        {
            private readonly UserDataContext _context;
            public InMemoryUserRepository(UserDataContext context)
            {
                _context = context;
            }

            public async Task<User> GetUserByUsernameAsync(string username)
            {
                return await _context.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Username == username);
            }

            public async Task<User> GetUserByIdAsync(int id)
            {
                return await _context.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Id == id);
            }

            public async Task<User> AddUserAsync(User user)
            {
                var added = await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();
                return added.Entity;
            }

            public async Task<User> UpdateUserAsync(User user)
            {
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return user;
            }

            public async Task<List<User>> GetAllUsersAsync()
            {
                return await _context.Users.Include(u => u.Roles).ToListAsync();
            }
        }

        private UserDataContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<UserDataContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new UserDataContext(options);
        }

        private IJwtTokenService CreateJwtTokenService()
        {
            var jwtSettings = Options.Create(new JwtSettings
            {
                Key = "supersecretkey_for_testing_purposes_only!",
                Issuer = "TestIssuer",
                Audience = "TestAudience",
                ExpiresInMinutes = 60
            });
            return new JwtTokenService(jwtSettings);
        }

        private User CreateTestUser(int id = 1, string username = "testuser", string password = "password123")
        {
            var role = new Role { Id = 1, Name = "User" };
            return new User
            {
                Id = id,
                Username = username,
                Password = password,
                Email = $"{username}@example.com",
                Roles = new List<Role> { role }
            };
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsTokenAndPublishesEvent()
        {
            var context = CreateInMemoryContext();

            var role = new Role { Id = 101, Name = "User" };
            await context.Roles.AddAsync(role);

            var user = CreateTestUser();
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var userRepository = new InMemoryUserRepository(context);
            var jwtTokenService = CreateJwtTokenService();

            var kafkaProducerMock = new Mock<UserKafkaProducer>("dummyBootstrapServers");
            kafkaProducerMock
                .Setup(k => k.PublishUserLoggedAsync(It.IsAny<UserLoggedEvent>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var loginService = new LoginService(jwtTokenService, userRepository, kafkaProducerMock.Object);

            var token = await loginService.Login(user.Username, user.Password);

            Assert.False(string.IsNullOrWhiteSpace(token));
            kafkaProducerMock.Verify(k => k.PublishUserLoggedAsync(It.Is<UserLoggedEvent>(e => e.UserId == user.Id && e.Email == user.Email)), Times.Once);
        }

        [Fact]
        public async Task Login_InvalidUsername_ThrowsInvalidCredentialsException()
        {
            var context = CreateInMemoryContext();
            var userRepository = new InMemoryUserRepository(context);
            var jwtTokenService = CreateJwtTokenService();
            var kafkaProducerMock = new Mock<UserKafkaProducer>("dummyBootstrapServers");

            var loginService = new LoginService(jwtTokenService, userRepository, kafkaProducerMock.Object);

            await Assert.ThrowsAsync<InvalidCredentialsException>(() => loginService.Login("unknownuser", "password"));
        }

        [Fact]
        public async Task Login_InvalidPassword_ThrowsInvalidCredentialsException()
        {
            var context = CreateInMemoryContext();

            var role = new Role { Id = 101, Name = "User" };
            await context.Roles.AddAsync(role);

            var user = CreateTestUser(password: "correctpassword");
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var userRepository = new InMemoryUserRepository(context);
            var jwtTokenService = CreateJwtTokenService();

            var kafkaProducerMock = new Mock<UserKafkaProducer>("dummyBootstrapServers");

            var loginService = new LoginService(jwtTokenService, userRepository, kafkaProducerMock.Object);

            await Assert.ThrowsAsync<InvalidCredentialsException>(() => loginService.Login(user.Username, "wrongpassword"));
        }

        [Fact]
        public async Task Login_UserWithMultipleRoles_ReturnsTokenAndPublishesEvent()
        {
            var context = CreateInMemoryContext();

            var role1 = new Role { Id = 1, Name = "User" };
            var role2 = new Role { Id = 2, Name = "Admin" };
            await context.Roles.AddRangeAsync(role1, role2);

            var user = new User
            {
                Id = 1,
                Username = "multiroleuser",
                Password = "password123",
                Email = "multi@example.com",
                Roles = new List<Role> { role1, role2 }
            };

            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var userRepository = new InMemoryUserRepository(context);
            var jwtTokenService = CreateJwtTokenService();
            var kafkaProducerMock = new Mock<UserKafkaProducer>("dummyBootstrapServers");
            kafkaProducerMock.Setup(k => k.PublishUserLoggedAsync(It.IsAny<UserLoggedEvent>())).Returns(Task.CompletedTask).Verifiable();

            var loginService = new LoginService(jwtTokenService, userRepository, kafkaProducerMock.Object);

            var token = await loginService.Login(user.Username, user.Password);

            Assert.False(string.IsNullOrWhiteSpace(token));
            kafkaProducerMock.Verify(k => k.PublishUserLoggedAsync(It.Is<UserLoggedEvent>(e => e.UserId == user.Id && e.Email == user.Email)), Times.Once);
        }

        [Fact]
        public async Task Login_EmptyUsername_ThrowsInvalidCredentialsException()
        {
            var context = CreateInMemoryContext();
            var userRepository = new InMemoryUserRepository(context);
            var jwtTokenService = CreateJwtTokenService();
            var kafkaProducerMock = new Mock<UserKafkaProducer>("dummyBootstrapServers");

            var loginService = new LoginService(jwtTokenService, userRepository, kafkaProducerMock.Object);

            await Assert.ThrowsAsync<InvalidCredentialsException>(() => loginService.Login("", "password"));
        }

        [Fact]
        public async Task Login_NullPassword_ThrowsInvalidCredentialsException()
        {
            var context = CreateInMemoryContext();
            var userRepository = new InMemoryUserRepository(context);
            var jwtTokenService = CreateJwtTokenService();
            var kafkaProducerMock = new Mock<UserKafkaProducer>("dummyBootstrapServers");

            var loginService = new LoginService(jwtTokenService, userRepository, kafkaProducerMock.Object);

            await Assert.ThrowsAsync<InvalidCredentialsException>(() => loginService.Login("testuser", null));
        }

        [Fact]
        public async Task Login_InactiveUser_ThrowsInvalidCredentialsException()
        {
            var context = CreateInMemoryContext();

            var role = new Role { Id = 101, Name = "User" };
            await context.Roles.AddAsync(role);
            await context.SaveChangesAsync();

            var user = CreateTestUser();
            user.IsActive = false;
            user.Roles.Add(role);

            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var userRepository = new InMemoryUserRepository(context);
            var jwtTokenService = CreateJwtTokenService();
            var kafkaProducerMock = new Mock<UserKafkaProducer>("dummyBootstrapServers");

            var loginService = new LoginService(jwtTokenService, userRepository, kafkaProducerMock.Object);

            await Assert.ThrowsAsync<InvalidCredentialsException>(() => loginService.Login(user.Username, user.Password));
        }


        [Fact]
        public async Task Login_WhitespacePassword_ThrowsInvalidCredentialsException()
        {
            var context = CreateInMemoryContext();

            var role = new Role { Id = 101, Name = "User" };
            await context.Roles.AddAsync(role);
            await context.SaveChangesAsync();

            var user = CreateTestUser(password: "password123");
            user.Roles.Add(role);

            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            var userRepository = new InMemoryUserRepository(context);
            var jwtTokenService = CreateJwtTokenService();
            var kafkaProducerMock = new Mock<UserKafkaProducer>("dummyBootstrapServers");

            var loginService = new LoginService(jwtTokenService, userRepository, kafkaProducerMock.Object);

            await Assert.ThrowsAsync<InvalidCredentialsException>(() => loginService.Login(user.Username, "   "));
        }


        [Fact]
        public async Task Login_SqlInjectionUsername_ThrowsInvalidCredentialsException()
        {
            var context = CreateInMemoryContext();
            var userRepository = new InMemoryUserRepository(context);
            var jwtTokenService = CreateJwtTokenService();
            var kafkaProducerMock = new Mock<UserKafkaProducer>("dummyBootstrapServers");

            var loginService = new LoginService(jwtTokenService, userRepository, kafkaProducerMock.Object);

            await Assert.ThrowsAsync<InvalidCredentialsException>(() => loginService.Login("'; DROP TABLE Users;--", "password"));
        }

        [Fact]
        public async Task Login_LongUsernamePassword_ThrowsInvalidCredentialsException()
        {
            var context = CreateInMemoryContext();
            var userRepository = new InMemoryUserRepository(context);
            var jwtTokenService = CreateJwtTokenService();
            var kafkaProducerMock = new Mock<UserKafkaProducer>("dummyBootstrapServers");

            var longUsername = new string('a', 1001);
            var longPassword = new string('b', 1001);

            var loginService = new LoginService(jwtTokenService, userRepository, kafkaProducerMock.Object);

            await Assert.ThrowsAsync<InvalidCredentialsException>(() => loginService.Login(longUsername, longPassword));
        }
    }
}
