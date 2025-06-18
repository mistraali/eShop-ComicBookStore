using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Threading.Tasks;
using UserService.Application.Kafka;
using UserService.Application.Services;
using UserService.Controllers;
using UserService.Domain.DTOs;
using UserService.Domain.Events;
using UserService.Domain.Exceptions;
using UserService.Domain.Models;
using UserService.Domain.Repositories;
using UserService.Domain.Seeders;
using Xunit;

namespace UserService.Tests.Integration
{
    public class LoginControllerTests : IDisposable
    {
        private readonly UserDataContext _context;
        private readonly LoginController _controller;
        private readonly LoginService _loginService;
        private readonly Mock<UserKafkaProducer> _kafkaMock;
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenService _jwtTokenService;

        public LoginControllerTests()
        {
            var options = new DbContextOptionsBuilder<UserDataContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _context = new UserDataContext(options);

            var seeder = new UserDbSeeder(_context);
            seeder.Seed().GetAwaiter().GetResult();

            _userRepository = new UserRepository(_context);

            _kafkaMock = new Mock<UserKafkaProducer>(null);
            _kafkaMock.Setup(k => k.PublishUserLoggedAsync(It.IsAny<UserLoggedEvent>()))
                .Returns(Task.CompletedTask);

            _jwtTokenService = new JwtTokenServiceFake();

            _loginService = new LoginService(_jwtTokenService, _userRepository, _kafkaMock.Object);
            _controller = new LoginController(_loginService, _userRepository);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsOkWithToken()
        {
            var dto = new UserLoginDto { Username = "admin", Password = "haslo" };
            var result = await _controller.Login(dto) as OkObjectResult;
            Assert.NotNull(result);
            Assert.NotNull(result.Value);
            Assert.Contains("token", result.Value.ToString(), StringComparison.OrdinalIgnoreCase);
            _kafkaMock.Verify(k => k.PublishUserLoggedAsync(It.Is<UserLoggedEvent>(e => e.Email == "admin@comicestore.pl")), Times.Once);
        }

        [Fact]
        public async Task Login_InvalidPassword_ReturnsUnauthorized()
        {
            var dto = new UserLoginDto { Username = "admin", Password = "wrong" };
            var result = await _controller.Login(dto);
            Assert.IsType<UnauthorizedResult>(result);
            _kafkaMock.Verify(k => k.PublishUserLoggedAsync(It.IsAny<UserLoggedEvent>()), Times.Never);
        }

        [Fact]
        public async Task Login_NonExistingUser_ReturnsUnauthorized()
        {
            var dto = new UserLoginDto { Username = "nonexistent", Password = "pass" };
            var result = await _controller.Login(dto);
            Assert.IsType<UnauthorizedResult>(result);
            _kafkaMock.Verify(k => k.PublishUserLoggedAsync(It.IsAny<UserLoggedEvent>()), Times.Never);
        }

        [Fact]
        public async Task Login_InactiveUser_ReturnsUnauthorized()
        {
            var inactiveUser = new User
            {
                Username = "inactive",
                Password = "pass",
                Email = "inactive@comicestore.pl",
                IsActive = false,
                CreatedAt = DateTime.UtcNow,
                Roles = await _context.Roles.ToListAsync()
            };
            _context.Users.Add(inactiveUser);
            await _context.SaveChangesAsync();

            var dto = new UserLoginDto { Username = "inactive", Password = "pass" };
            var result = await _controller.Login(dto);
            Assert.IsType<UnauthorizedResult>(result);
            _kafkaMock.Verify(k => k.PublishUserLoggedAsync(It.IsAny<UserLoggedEvent>()), Times.Never);
        }

        [Theory]
        [InlineData(null, "haslo")]
        [InlineData("admin", null)]
        [InlineData(null, null)]
        [InlineData("", "haslo")]
        [InlineData("admin", "")]
        public async Task Login_MissingCredentials_ReturnsUnauthorized(string username, string password)
        {
            var dto = new UserLoginDto { Username = username, Password = password };
            var result = await _controller.Login(dto);
            Assert.IsType<UnauthorizedResult>(result);
            _kafkaMock.Verify(k => k.PublishUserLoggedAsync(It.IsAny<UserLoggedEvent>()), Times.Never);
        }

        [Fact]
        public async Task Login_UsernameCaseSensitive_ReturnsUnauthorized()
        {
            var dto = new UserLoginDto { Username = "Admin", Password = "haslo" };
            var result = await _controller.Login(dto);
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task Login_PasswordCaseSensitive_ReturnsUnauthorized()
        {
            var dto = new UserLoginDto { Username = "admin", Password = "Haslo" };
            var result = await _controller.Login(dto);
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task Login_EventIsFiredOnlyOnSuccess()
        {
            var validDto = new UserLoginDto { Username = "admin", Password = "haslo" };
            var invalidDto = new UserLoginDto { Username = "admin", Password = "wrong" };

            var resultSuccess = await _controller.Login(validDto);
            var resultFail = await _controller.Login(invalidDto);

            Assert.IsType<OkObjectResult>(resultSuccess);
            Assert.IsType<UnauthorizedResult>(resultFail);

            _kafkaMock.Verify(k => k.PublishUserLoggedAsync(It.IsAny<UserLoggedEvent>()), Times.Once);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        // Minimal fake JWT service for testing
        private class JwtTokenServiceFake : IJwtTokenService
        {
            public string GenerateToken(int userId, System.Collections.Generic.List<string> roles)
            {
                return "fake-jwt-token";
            }
        }
    }
}
