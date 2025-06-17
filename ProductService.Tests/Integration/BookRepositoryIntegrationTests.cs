using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Models;
using ProductService.Domain.Repositories;
using Xunit;

namespace ProductService.Tests.Integration;
public class BookRepositoryIntegrationTests
{
    private ProductContext GetInMemoryContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ProductContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new ProductContext(options);
    }

    private Book CreateSampleBook(int id = 1, int categoryId = 1)
    {
        return new Book
        {
            Id = id,
            Name = "Sample Book",
            Author = "Author",
            Price = 10.99m,
            Stock = 5,
            PublisherId = 1,
            ReleaseYear = 2020,
            Ean = "1234567890123",
            Isbn = "978-3-16-148410-0",
            Sku = "SKU001",
            CategoryId = categoryId
        };
    }

    [Fact]
    public async Task AddAsync_ShouldAddBookToDatabase()
    {
        var context = GetInMemoryContext(nameof(AddAsync_ShouldAddBookToDatabase));
        var repo = new BookRepository(context);

        var book = CreateSampleBook();

        await repo.AddAsync(book);

        var booksInDb = await context.Books.ToListAsync();
        Assert.Single(booksInDb);
        Assert.Equal("Sample Book", booksInDb.First().Name);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllBooks()
    {
        var context = GetInMemoryContext(nameof(GetAllAsync_ShouldReturnAllBooks));
        var repo = new BookRepository(context);

        context.Books.AddRange(CreateSampleBook(1), CreateSampleBook(2));
        await context.SaveChangesAsync();

        var books = await repo.GetAllAsync();

        Assert.Equal(2, books.Count());
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCorrectBook()
    {
        var context = GetInMemoryContext(nameof(GetByIdAsync_ShouldReturnCorrectBook));
        var repo = new BookRepository(context);

        var book = CreateSampleBook(123);
        context.Books.Add(book);
        await context.SaveChangesAsync();

        var foundBook = await repo.GetByIdAsync(123);

        Assert.NotNull(foundBook);
        Assert.Equal(123, foundBook.Id);
        Assert.Equal("Sample Book", foundBook.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenBookNotFound()
    {
        var context = GetInMemoryContext(nameof(GetByIdAsync_ShouldReturnNull_WhenBookNotFound));
        var repo = new BookRepository(context);

        var result = await repo.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_ShouldModifyExistingBook()
    {
        var context = GetInMemoryContext(nameof(UpdateAsync_ShouldModifyExistingBook));
        var repo = new BookRepository(context);

        var book = CreateSampleBook(1);
        context.Books.Add(book);
        await context.SaveChangesAsync();

        book.Name = "Updated Name";

        await repo.UpdateAsync(book);

        var updatedBook = await context.Books.FindAsync(1);
        Assert.Equal("Updated Name", updatedBook.Name);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveBook()
    {
        var context = GetInMemoryContext(nameof(DeleteAsync_ShouldRemoveBook));
        var repo = new BookRepository(context);

        var book = CreateSampleBook(1);
        context.Books.Add(book);
        await context.SaveChangesAsync();

        await repo.DeleteAsync(book);

        var booksInDb = await context.Books.ToListAsync();
        Assert.Empty(booksInDb);
    }

    [Fact]
    public async Task DeleteAsync_ShouldNotThrow_WhenBookNotInDb()
    {
        var context = GetInMemoryContext(nameof(DeleteAsync_ShouldNotThrow_WhenBookNotInDb));
        var repo = new BookRepository(context);

        var book = CreateSampleBook(999); 
        var exception = await Record.ExceptionAsync(() => repo.DeleteAsync(book));

        Assert.Null(exception);
    }
}
