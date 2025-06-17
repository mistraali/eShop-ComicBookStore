using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using ProductService.Domain.DTOs;
using ProductService.Domain.Models;
using Xunit;

namespace ProductService.Tests.Integration;

public class BooksControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public BooksControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetBooks_ReturnsSeededBooks()
    {
        var response = await _client.GetAsync("/api/books");
        response.EnsureSuccessStatusCode();

        var books = await response.Content.ReadFromJsonAsync<List<BookDto>>();
        Assert.NotNull(books);
        Assert.NotEmpty(books);
        Assert.Contains(books, b => b.Name.Contains("Batman"));
        Assert.Contains(books, b => b.Name.Contains("Dragon Ball"));
    }

    [Fact]
    public async Task GetBook_ById_ReturnsCorrectBook()
    {
        var response = await _client.GetAsync("/api/books/2");
        response.EnsureSuccessStatusCode();

        var book = await response.Content.ReadFromJsonAsync<BookDto>();
        Assert.NotNull(book);
        Assert.Equal(2, book.Id);
        Assert.Equal("Dragon Ball Vol. 1", book.Name);
    }

    [Fact]
    public async Task GetBook_NonExistingId_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/books/9999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PostBook_CreatesBookSuccessfully()
    {
        var newBook = new Book
        {
            Name = "Test Book",
            Author = "Test Author",
            Price = 9.99m,
            Stock = 5,
            PublisherId = 99,
            ReleaseYear = 2025,
            Ean = "0000000000000",
            Isbn = "0000000000",
            Sku = "TEST-SKU"
        };

        var postResponse = await _client.PostAsJsonAsync("/api/books", newBook);
        postResponse.EnsureSuccessStatusCode();

        var createdBook = await postResponse.Content.ReadFromJsonAsync<BookDto>();
        Assert.NotNull(createdBook);
        Assert.Equal(newBook.Name, createdBook.Name);

        // Cleanup
        await _client.DeleteAsync($"/api/books/{createdBook.Id}");
    }

    [Fact]
    public async Task PostBook_InvalidModel_ReturnsBadRequest()
    {
        var invalidBook = new Book
        {
            // Załóżmy, że Name jest wymagane i nie podajemy go tutaj
            Author = "Author",
            Price = 10,
            Stock = 1,
            PublisherId = 1,
            ReleaseYear = 2025,
            Ean = "0000000000000",
            Isbn = "0000000000",
            Sku = "SKU"
        };

        var response = await _client.PostAsJsonAsync("/api/books", invalidBook);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PutBook_UpdatesBookSuccessfully()
    {
        var getResponse = await _client.GetAsync("/api/books/1");
        getResponse.EnsureSuccessStatusCode();

        var book = await getResponse.Content.ReadFromJsonAsync<BookDto>();
        Assert.NotNull(book);

        var updatedBook = new Book
        {
            Id = book.Id,
            Name = book.Name + " Updated",
            Author = book.Author,
            Price = book.Price,
            Stock = book.Stock,
            PublisherId = book.PublisherId,
            ReleaseYear = book.ReleaseYear,
            Ean = book.Ean,
            Isbn = book.Isbn,
            Sku = book.Sku
        };

        var putResponse = await _client.PutAsJsonAsync($"/api/books/{book.Id}", updatedBook);
        Assert.Equal(HttpStatusCode.NoContent, putResponse.StatusCode);

        var verifyResponse = await _client.GetAsync($"/api/books/{book.Id}");
        verifyResponse.EnsureSuccessStatusCode();

        var updatedBookDto = await verifyResponse.Content.ReadFromJsonAsync<BookDto>();
        Assert.NotNull(updatedBookDto);
        Assert.EndsWith("Updated", updatedBookDto.Name);
    }

    [Fact]
    public async Task PutBook_NonMatchingId_ReturnsBadRequest()
    {
        var book = new Book
        {
            Id = 1,
            Name = "Invalid Update",
            Author = "Author",
            Price = 10,
            Stock = 1,
            PublisherId = 1,
            ReleaseYear = 2025,
            Ean = "0000000000000",
            Isbn = "0000000000",
            Sku = "SKU"
        };

        var response = await _client.PutAsJsonAsync("/api/books/2", book);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PutBook_NonExistingId_ReturnsNotFound()
    {
        var book = new Book
        {
            Id = 9999,
            Name = "Non-existing book",
            Author = "Author",
            Price = 10,
            Stock = 1,
            PublisherId = 1,
            ReleaseYear = 2025,
            Ean = "0000000000000",
            Isbn = "0000000000",
            Sku = "SKU"
        };

        var response = await _client.PutAsJsonAsync("/api/books/9999", book);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteBook_DeletesBookSuccessfully()
    {
        var newBook = new Book
        {
            Name = "Book to Delete",
            Author = "Author",
            Price = 5,
            Stock = 1,
            PublisherId = 1,
            ReleaseYear = 2023,
            Ean = "1111111111111",
            Isbn = "1111111111",
            Sku = "DELETE-SKU"
        };

        var postResponse = await _client.PostAsJsonAsync("/api/books", newBook);
        postResponse.EnsureSuccessStatusCode();

        var createdBook = await postResponse.Content.ReadFromJsonAsync<BookDto>();
        Assert.NotNull(createdBook);

        var deleteResponse = await _client.DeleteAsync($"/api/books/{createdBook.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/books/{createdBook.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteBook_NonExistingId_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync("/api/books/9999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
