using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProductService;
using ProductService.Domain.DTOs;
using ProductService.Domain.Models;
using ProductService.Domain.Repositories;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace ProductService.Tests.E2E;
public class BooksE2ETests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public BooksE2ETests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Usuń istniejący DbContext i podmień na InMemory
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ProductContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddDbContext<ProductContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                });

                // Zbuduj provider i stwórz scope, by wywołać EnsureCreated()
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ProductContext>();

                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                // NIE seeduj ręcznie danych — zadziała seed z HasData() w OnModelCreating
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetBooks_ReturnsSeededBooks()
    {
        var response = await _client.GetAsync("/api/books");
        response.EnsureSuccessStatusCode();

        var books = await response.Content.ReadFromJsonAsync<List<BookDto>>();
        Assert.NotNull(books);
        Assert.True(books.Count >= 2);
        Assert.Contains(books, b => b.Name == "Batman: Year One");
    }

    [Fact]
    public async Task GetBook_ById_ReturnsCorrectBook()
    {
        var response = await _client.GetAsync("/api/books/1");
        response.EnsureSuccessStatusCode();

        var book = await response.Content.ReadFromJsonAsync<BookDto>();
        Assert.NotNull(book);
        Assert.Equal(1, book.Id);
        Assert.Equal("Batman: Year One", book.Name);
    }

    [Fact]
    public async Task PostBook_CreatesNewBook()
    {
        var newBook = new Book
        {
            Name = "New Book",
            Author = "New Author",
            Price = 9.99m,
            Stock = 5,
            PublisherId = 1,
            ReleaseYear = 2023,
            Ean = "1111111111111",
            Isbn = "1234567890",
            Sku = "NEW-SKU",
            CategoryId = 1
        };

        var postResponse = await _client.PostAsJsonAsync("/api/books", newBook);
        postResponse.EnsureSuccessStatusCode();

        var createdBook = await postResponse.Content.ReadFromJsonAsync<BookDto>();
        Assert.NotNull(createdBook);
        Assert.Equal(newBook.Name, createdBook.Name);
        Assert.True(createdBook.Id > 0);
    }

    [Fact]
    public async Task PutBook_UpdatesExistingBook()
    {
        var getResponse = await _client.GetAsync("/api/books/1");
        getResponse.EnsureSuccessStatusCode();

        var book = await getResponse.Content.ReadFromJsonAsync<BookDto>();
        Assert.NotNull(book);

        // Ustaw domyślną kategorię, jeśli CategoryId jest 0 lub mniejsze
        var categoryId = book.CategoryId > 0 ? book.CategoryId : 1;

        var updatedBook = new Book
        {
            Id = book.Id,
            Name = book.Name + " Updated",
            Author = book.Author,
            Price = book.Price + 1,
            Stock = book.Stock + 1,
            PublisherId = book.PublisherId,
            ReleaseYear = book.ReleaseYear,
            Ean = book.Ean,
            Isbn = book.Isbn,
            Sku = book.Sku,
            CategoryId = categoryId
        };

        var putResponse = await _client.PutAsJsonAsync($"/api/books/{updatedBook.Id}", updatedBook);

        if (putResponse.StatusCode != HttpStatusCode.NoContent)
        {
            var errorContent = await putResponse.Content.ReadAsStringAsync();
            throw new Exception($"PUT failed with status {putResponse.StatusCode}: {errorContent}");
        }

        Assert.Equal(HttpStatusCode.NoContent, putResponse.StatusCode);

        var getAfterPut = await _client.GetAsync($"/api/books/{updatedBook.Id}");
        getAfterPut.EnsureSuccessStatusCode();

        var bookAfterPut = await getAfterPut.Content.ReadFromJsonAsync<BookDto>();
        Assert.NotNull(bookAfterPut);
        Assert.Equal(updatedBook.Name, bookAfterPut.Name);
    }



    [Fact]
    public async Task DeleteBook_DeletesBook()
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
            Sku = "DELETE-SKU",
            CategoryId = 1
        };

        var postResponse = await _client.PostAsJsonAsync("/api/books", newBook);
        postResponse.EnsureSuccessStatusCode();

        var createdBook = await postResponse.Content.ReadFromJsonAsync<BookDto>();

        var deleteResponse = await _client.DeleteAsync($"/api/books/{createdBook.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/books/{createdBook.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }
}
