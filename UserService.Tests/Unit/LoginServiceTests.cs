using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using UserService.Application.Kafka;
using UserService.Application.Services;
using UserService.Domain.Events;
using UserService.Domain.Exceptions;
using UserService.Domain.Models;
using UserService.Domain.Repositories;
using Xunit;

namespace UserService.Tests.Unit
{
    public class LoginServiceTests
    {
        private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<UserKafkaProducer> _kafkaProducerMock;
        private readonly LoginService _loginService;

        public LoginServiceTests()
        {
            _jwtTokenServiceMock = new Mock<IJwtTokenService>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _kafkaProducerMock = new Mock<UserKafkaProducer>(MockBehavior.Strict, "dummy:9092");

            _loginService = new LoginService(
                _jwtTokenServiceMock.Object,
                _userRepositoryMock.Object,
                _kafkaProducerMock.Object);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsTokenAndPublishesEvent()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Email = "test@example.com",
                Password = "password123",
                Roles = new List<Role> { new Role { Name = "Client" } }
            };

            _userRepositoryMock
                .Setup(r => r.GetUserByUsernameAsync("testuser"))
                .ReturnsAsync(user);

            _jwtTokenServiceMock
                .Setup(j => j.GenerateToken(user.Id, It.Is<List<string>>(roles => roles.Contains("Client"))))
                .Returns("valid.jwt.token");

            _kafkaProducerMock
                .Setup(k => k.PublishUserLoggedAsync(It.Is<UserLoggedEvent>(e => e.UserId == user.Id && e.Email == user.Email)))
                .Returns(Task.CompletedTask);

            // Act
            var token = await _loginService.Login("testuser", "password123");

            // Assert
            Assert.Equal("valid.jwt.token", token);
            _userRepositoryMock.Verify(r => r.GetUserByUsernameAsync("testuser"), Times.Once);
            _jwtTokenServiceMock.Verify(j => j.GenerateToken(user.Id, It.IsAny<List<string>>()), Times.Once);
            _kafkaProducerMock.Verify(k => k.PublishUserLoggedAsync(It.IsAny<UserLoggedEvent>()), Times.Once);
        }

        [Fact]
        public async Task Login_InvalidUsername_ThrowsInvalidCredentialsException()
        {
            // Arrange
            _userRepositoryMock
                .Setup(r => r.GetUserByUsernameAsync("wronguser"))
                .ReturnsAsync((User)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidCredentialsException>(() => _loginService.Login("wronguser", "password"));
            _userRepositoryMock.Verify(r => r.GetUserByUsernameAsync("wronguser"), Times.Once);
            _jwtTokenServiceMock.Verify(j => j.GenerateToken(It.IsAny<int>(), It.IsAny<List<string>>()), Times.Never);
            _kafkaProducerMock.Verify(k => k.PublishUserLoggedAsync(It.IsAny<UserLoggedEvent>()), Times.Never);
        }

        [Fact]
        public async Task Login_WrongPassword_ThrowsInvalidCredentialsException()
        {
            // Arrange
            var user = new User
            {
                Id = 2,
                Email = "test2@example.com",
                Password = "correctPassword",
                Roles = new List<Role> { new Role { Name = "Admin" } }
            };

            _userRepositoryMock
                .Setup(r => r.GetUserByUsernameAsync("user2"))
                .ReturnsAsync(user);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidCredentialsException>(() => _loginService.Login("user2", "wrongPassword"));
            _userRepositoryMock.Verify(r => r.GetUserByUsernameAsync("user2"), Times.Once);
            _jwtTokenServiceMock.Verify(j => j.GenerateToken(It.IsAny<int>(), It.IsAny<List<string>>()), Times.Never);
            _kafkaProducerMock.Verify(k => k.PublishUserLoggedAsync(It.IsAny<UserLoggedEvent>()), Times.Never);
        }
    }
}
