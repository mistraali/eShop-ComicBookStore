using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using UserService.Application.Services;
using UserService.Domain.JWT;
using Xunit;

namespace UserService.Tests.Unit
{
    public class JwtTokenServiceTests
    {
        private readonly JwtTokenService _service;
        private readonly JwtSettings _settings;

        public JwtTokenServiceTests()
        {
            _settings = new JwtSettings
            {
                Key = "very_long_secure_key_for_testing_purposes_1234567890",
                Issuer = "TestIssuer",
                Audience = "TestAudience",
                ExpiresInMinutes = 60
            };
            var options = Options.Create(_settings);
            _service = new JwtTokenService(options);
        }

        [Fact]
        public void GenerateToken_ReturnsNonEmptyString()
        {
            var token = _service.GenerateToken(1, new List<string> { "Admin", "User" });
            Assert.False(string.IsNullOrEmpty(token));
        }

        [Fact]
        public void GenerateToken_IncludesUserIdAndRolesClaims()
        {
            int userId = 42;
            var roles = new List<string> { "Admin", "User" };

            var token = _service.GenerateToken(userId, roles);

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            var userIdClaim = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            Assert.NotNull(userIdClaim);
            Assert.Equal(userId.ToString(), userIdClaim.Value);

            var roleClaims = jwt.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
            Assert.Equal(roles.OrderBy(r => r), roleClaims.OrderBy(r => r));
        }

        [Fact]
        public void GenerateToken_HasCorrectIssuerAndAudience()
        {
            var token = _service.GenerateToken(1, new List<string>());

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            Assert.Equal(_settings.Issuer, jwt.Issuer);

            var audienceClaim = jwt.Claims.FirstOrDefault(c => c.Type == "aud");
            Assert.NotNull(audienceClaim);
            Assert.Equal(_settings.Audience, audienceClaim.Value);
        }

        [Fact]
        public void GenerateToken_HasExpiryCloseToSettings()
        {
            var before = DateTime.UtcNow;
            var token = _service.GenerateToken(1, new List<string>());
            var after = DateTime.UtcNow;

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            Assert.True(jwt.ValidTo > before.AddMinutes(_settings.ExpiresInMinutes - 1));
            Assert.True(jwt.ValidTo <= after.AddMinutes(_settings.ExpiresInMinutes + 1));
        }

        [Fact]
        public void Constructor_ThrowsIfSettingsIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new JwtTokenService(null));
        }

        [Fact]
        public void GenerateToken_ThrowsIfKeyIsInvalid()
        {
            var badSettings = new JwtSettings
            {
                Key = "short",
                Issuer = "Issuer",
                Audience = "Audience",
                ExpiresInMinutes = 60
            };
            var options = Options.Create(badSettings);
            var service = new JwtTokenService(options);

            // The exception is thrown during signing credentials setup or token creation
            Assert.ThrowsAny<ArgumentException>(() => service.GenerateToken(1, new List<string>()));
        }
    }
}
