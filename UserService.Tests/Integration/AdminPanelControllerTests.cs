using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using UserService.Application.Services;
using UserService.Domain.DTOs;
using UserService.Domain.JWT;
using UserService.Domain.Models;
using UserService.Domain.Seeders;
using Xunit;

namespace UserService.Tests.Integration;

public class AdminPanelControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly JwtSettings _jwtSettings;

    public AdminPanelControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped<IUserDbSeeder, UserDbSeeder>();
            });
        });

        using var scope = _factory.Services.CreateScope();
        _jwtSettings = scope.ServiceProvider.GetRequiredService<IOptions<JwtSettings>>().Value;
    }

    private HttpClient CreateClientWithAdminToken()
    {
        var client = _factory.CreateClient();
        var token = GenerateJwtToken(1, new List<string> { "Admin" });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private HttpClient CreateClientWithUserToken()
    {
        var client = _factory.CreateClient();
        var token = GenerateJwtToken(3, new List<string> { "Client" });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private string GenerateJwtToken(int userId, List<string> roles)
    {
        var service = new JwtTokenService(Options.Create(_jwtSettings));
        return service.GenerateToken(userId, roles);
    }

    [Fact]
    public async Task GetAllUsers_AsAdmin_ReturnsOk()
    {
        var client = CreateClientWithAdminToken();
        var response = await client.GetAsync("/api/AdminPanel");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetAllUsers_AsClient_ReturnsForbidden()
    {
        var client = CreateClientWithUserToken();
        var response = await client.GetAsync("/api/AdminPanel");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetUserById_ExistingUser_AsAdmin_ReturnsOk()
    {
        var client = CreateClientWithAdminToken();
        var response = await client.GetAsync("/api/AdminPanel/1");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetUserById_NonExistingUser_AsAdmin_ReturnsNotFound()
    {
        var client = CreateClientWithAdminToken();
        var response = await client.GetAsync("/api/AdminPanel/9999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetUserById_AsClient_ReturnsForbidden()
    {
        var client = CreateClientWithUserToken();
        var response = await client.GetAsync("/api/AdminPanel/1");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task EditUserAccount_AsAdmin_ValidRequest_ReturnsOk()
    {
        var client = CreateClientWithAdminToken();
        var dto = new UserUpdateDto
        {
            NewUsername = "new_admin",
            NewEmail = "new_admin@comicestore.pl",
            NewIsActive = true,
            NewPassword = "newpassword",
            NewRoleIds = new List<int> { 1 }
        };

        var response = await client.PutAsJsonAsync("/api/AdminPanel/edit-user-account/1", dto);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task EditUserAccount_AsAdmin_ServerError_ReturnsBadRequest()
    {
        var client = CreateClientWithAdminToken();
        var dto = new UserUpdateDto
        {
            NewUsername = "",
            NewEmail = "",
            NewIsActive = true,
            NewPassword = "",
            NewRoleIds = new List<int> { 999 }
        };

        var response = await client.PutAsJsonAsync("/api/AdminPanel/edit-user-account/1", dto);
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task EditUserAccount_AsAdmin_InvalidUpdate_ReturnsInternalServerError()
    {
        var client = CreateClientWithAdminToken();
        var dto = new UserUpdateDto
        {
            NewUsername = null,
            NewEmail = null,
            NewIsActive = true,
            NewPassword = null,
            NewRoleIds = null
        };

        var response = await client.PutAsJsonAsync("/api/AdminPanel/edit-user-account/1", dto);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task EditUserAccount_AsClient_ReturnsForbidden()
    {
        var client = CreateClientWithUserToken();
        var dto = new UserUpdateDto
        {
            NewUsername = "hack",
            NewEmail = "hack@me.pl",
            NewIsActive = true,
            NewPassword = "hack",
            NewRoleIds = new List<int> { 1 }
        };

        var response = await client.PutAsJsonAsync("/api/AdminPanel/edit-user-account/1", dto);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
