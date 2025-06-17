using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using ProductService.Domain.DTOs;
using ProductService.Domain.Models;
using Xunit;

namespace ProductService.Tests.E2E;

public class CategoryControllerE2ETests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public CategoryControllerE2ETests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ReturnsCategories()
    {
        var response = await _client.GetAsync("/api/categories");
        response.EnsureSuccessStatusCode();

        var categories = await response.Content.ReadFromJsonAsync<List<Category>>();
        Assert.NotNull(categories);
        Assert.NotEmpty(categories);
        Assert.Contains(categories, c => !string.IsNullOrEmpty(c.Name));
    }

    [Fact]
    public async Task GetById_Existing_ReturnsCategory()
    {
        var response = await _client.GetAsync("/api/categories/1");
        response.EnsureSuccessStatusCode();

        var category = await response.Content.ReadFromJsonAsync<Category>();
        Assert.NotNull(category);
        Assert.Equal(1, category.Id);
    }

    [Fact]
    public async Task GetById_NonExisting_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/categories/9999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetBooksByCategory_ReturnsBooks()
    {
        var response = await _client.GetAsync("/api/categories/1/books");
        response.EnsureSuccessStatusCode();

        var books = await response.Content.ReadFromJsonAsync<List<BookDto>>();
        Assert.NotNull(books);
        Assert.All(books, b => Assert.Equal(1, b.CategoryId));
    }

    [Fact]
    public async Task Create_Update_Delete_Category_WorkCorrectly()
    {
        // Create
        var newCategory = new Category
        {
            Name = "Test Category",
            Description = "Test description"
        };

        var postResponse = await _client.PostAsJsonAsync("/api/categories", newCategory);
        postResponse.EnsureSuccessStatusCode();

        var createdCategory = await postResponse.Content.ReadFromJsonAsync<Category>();
        Assert.NotNull(createdCategory);
        Assert.Equal(newCategory.Name, createdCategory.Name);

        // Update
        createdCategory.Name = "Updated Test Category";
        var putResponse = await _client.PutAsJsonAsync($"/api/categories/{createdCategory.Id}", createdCategory);
        Assert.Equal(HttpStatusCode.NoContent, putResponse.StatusCode);

        var getUpdatedResponse = await _client.GetAsync($"/api/categories/{createdCategory.Id}");
        getUpdatedResponse.EnsureSuccessStatusCode();
        var updatedCategory = await getUpdatedResponse.Content.ReadFromJsonAsync<Category>();
        Assert.Equal("Updated Test Category", updatedCategory.Name);

        // Delete
        var deleteResponse = await _client.DeleteAsync($"/api/categories/{createdCategory.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getDeletedResponse = await _client.GetAsync($"/api/categories/{createdCategory.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getDeletedResponse.StatusCode);
    }

    [Fact]
    public async Task Update_WithIdMismatch_ReturnsBadRequest()
    {
        var category = new Category { Id = 1, Name = "Mismatch" };
        var response = await _client.PutAsJsonAsync("/api/categories/2", category);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Delete_NonExisting_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync("/api/categories/9999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
