using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Repositories;
using ProductService.Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace ProductService.Tests.Integration;
public class BookServiceIntegrationTests
{
    private ProductContext GetContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ProductContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new ProductContext(options);
    }

    private Book CreateValidBook(int? id = null, int categoryId = 1)
    {
        return new Book
        {
            Id = id ?? 0,
            Name = "Test Book",
            Author = "Test Author",
            Price = 19.99m,
            Stock = 5,
            PublisherId = 1,
            ReleaseYear = 2021,
            Ean = "1234567890123",
            Isbn = "1234567890123",
            Sku = "TESTSKU",
            CategoryId = categoryId
        };
    }

    private List<ValidationResult> ValidateModel(Book book)
    {
        var context = new ValidationContext(book, null, null);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(book, context, results, true);
        return results;
    }

    [Fact]
    public async Task AddAsync_AddsBookToDb()
    {
        var context = GetContext(nameof(AddAsync_AddsBookToDb));
        var repo = new BookRepository(context);

        var book = CreateValidBook();

        await repo.AddAsync(book);

        Assert.Single(context.Books);
        Assert.Equal("Test Book", context.Books.First().Name);
        Assert.Equal(1, context.Books.First().CategoryId);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllBooks()
    {
        var context = GetContext(nameof(GetAllAsync_ReturnsAllBooks));
        context.Books.AddRange(
            CreateValidBook(1, 2),
            CreateValidBook(2, 3)
        );
        context.SaveChanges();

        var repo = new BookRepository(context);
        var result = await repo.GetAllAsync();

        Assert.Equal(2, result.Count());
        Assert.Contains(result, b => b.CategoryId == 2);
        Assert.Contains(result, b => b.CategoryId == 3);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsCorrectBook()
    {
        var context = GetContext(nameof(GetByIdAsync_ReturnsCorrectBook));
        var book = CreateValidBook(123, 5);
        context.Books.Add(book);
        context.SaveChanges();

        var repo = new BookRepository(context);
        var result = await repo.GetByIdAsync(123);

        Assert.NotNull(result);
        Assert.Equal("Test Book", result?.Name);
        Assert.Equal(5, result?.CategoryId);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesBook()
    {
        var context = GetContext(nameof(UpdateAsync_UpdatesBook));
        var book = CreateValidBook(1, 1);
        context.Books.Add(book);
        context.SaveChanges();

        var repo = new BookRepository(context);
        book.Name = "Updated Book";
        book.CategoryId = 2;
        await repo.UpdateAsync(book);

        var updated = await context.Books.FindAsync(1);
        Assert.Equal("Updated Book", updated?.Name);
        Assert.Equal(2, updated?.CategoryId);
    }

    [Fact]
    public async Task DeleteAsync_RemovesBook()
    {
        var context = GetContext(nameof(DeleteAsync_RemovesBook));
        var book = CreateValidBook(2, 1);
        context.Books.Add(book);
        context.SaveChanges();

        var repo = new BookRepository(context);
        await repo.DeleteAsync(book);

        Assert.Empty(context.Books);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_IfNotFound()
    {
        var context = GetContext(nameof(GetByIdAsync_ReturnsNull_IfNotFound));
        var repo = new BookRepository(context);

        var result = await repo.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public void Book_WithInvalidIsbnLength_FailsValidation()
    {
        var book = CreateValidBook();
        book.Isbn = "123"; // too short

        var results = ValidateModel(book);

        Assert.Contains(results, r => r.MemberNames.Contains("Isbn"));
    }

    [Fact]
    public void Book_WithInvalidPrice_FailsValidation()
    {
        var book = CreateValidBook();
        book.Price = 0; // too low

        var results = ValidateModel(book);

        Assert.Contains(results, r => r.MemberNames.Contains("Price"));
    }

    [Fact]
    public void Book_WithInvalidReleaseYear_FailsValidation()
    {
        var book = CreateValidBook();
        book.ReleaseYear = 1800; // too early

        var results = ValidateModel(book);

        Assert.Contains(results, r => r.MemberNames.Contains("ReleaseYear"));
    }

    [Fact]
    public void Book_WithMissingCategoryId_FailsValidation()
    {
        var book = CreateValidBook();
        book.CategoryId = 0; // assuming 0 is invalid, or make CategoryId nullable if optional

        var results = ValidateModel(book);

        Assert.Contains(results, r => r.MemberNames.Contains("CategoryId"));
    }
}
