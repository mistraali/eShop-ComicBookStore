using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Xunit;
using UserService.Domain.Seeders;
using UserService.Domain.Repositories;
using UserService.Application.Services;
using UserService.Domain.DTOs;

namespace UserService.Tests.Integration
{
    public class RegisterControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public RegisterControllerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<UserDataContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    services.AddDbContext<UserDataContext>(options =>
                        options.UseInMemoryDatabase("TestDb_RegisterController"));

                    services.AddScoped<IUserDbSeeder, UserDbSeeder>();
                    services.AddScoped<IUserRepository, UserRepository>();
                    services.AddScoped<IRoleRepository, RoleRepository>();
                    services.AddScoped<IUserService, UserService.Application.Services.UserService>();

                    var sp = services.BuildServiceProvider();
                    using var scope = sp.CreateScope();
                    var scopedServices = scope.ServiceProvider;
                    var context = scopedServices.GetRequiredService<UserDataContext>();
                    context.Database.EnsureDeleted();
                    context.Database.EnsureCreated();

                    var seeder = scopedServices.GetRequiredService<IUserDbSeeder>();
                    seeder.Seed().Wait();
                });
            });
        }

        [Fact]
        public async Task Post_RegisterNewUser_ReturnsOkAndUserIsAdded()
        {
            var client = _factory.CreateClient();

            var newUser = new UserRegisterDto
            {
                Username = "newuser",
                Email = "newuser@example.com",
                Password = "password123"
            };

            var response = await client.PostAsJsonAsync("/api/register", newUser);

            response.EnsureSuccessStatusCode();

            var createdUser = await response.Content.ReadFromJsonAsync<UserReadDto>();

            Assert.NotNull(createdUser);
            Assert.Equal(newUser.Username, createdUser.Username);
            Assert.Equal(newUser.Email, createdUser.Email);
            Assert.Contains("Client", createdUser.Roles);

            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<UserDataContext>();
            var userInDb = await context.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Username == newUser.Username);

            Assert.NotNull(userInDb);
            Assert.Equal(newUser.Email, userInDb.Email);
            Assert.Contains(userInDb.Roles, r => r.Name == "Client");
        }
    }
}
