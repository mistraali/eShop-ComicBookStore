using Moq;
using ProductService.Application.Services;
using ProductService.Domain.Models;
using ProductService.Domain.Repositories;

namespace ProductService.Tests.Unit;

public class BookControllerTests
{
    private readonly Mock<IBookRepository> _bookRepositoryMock;
    private readonly BookService _bookService;

    public BookControllerTests()
    {
        _bookRepositoryMock = new Mock<IBookRepository>();
        _bookService = new BookService(_bookRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateBookAsync_ShouldAddBookAndReturnDto()
    {
        var book = GetSampleBook();
        _bookRepositoryMock.Setup(r => r.AddAsync(book)).Returns(Task.CompletedTask);

        var result = await _bookService.CreateBookAsync(book);

        _bookRepositoryMock.Verify(r => r.AddAsync(book), Times.Once);
        Assert.NotNull(result);
        Assert.Equal(book.Id, result.Id);
        Assert.Equal(book.Name, result.Name);
    }

    [Fact]
    public async Task CreateBookAsync_ShouldThrow_WhenBookIsNull()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _bookService.CreateBookAsync(null));
    }

    [Fact]
    public async Task GetAllBooksAsync_ShouldReturnMappedDtos()
    {
        var books = new List<Book> { GetSampleBook() };
        _bookRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(books);

        var result = await _bookService.GetAllBooksAsync();

        Assert.Single(result);
        Assert.Equal(books[0].Id, result[0].Id);
    }

    [Fact]
    public async Task GetBookByIdAsync_ShouldReturnDto_WhenBookExists()
    {
        var book = GetSampleBook();
        _bookRepositoryMock.Setup(r => r.GetByIdAsync(book.Id)).ReturnsAsync(book);

        var result = await _bookService.GetBookByIdAsync(book.Id);

        Assert.NotNull(result);
        Assert.Equal(book.Id, result.Id);
    }

    [Fact]
    public async Task GetBookByIdAsync_ShouldThrow_WhenBookNotFound()
    {
        _bookRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Book)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _bookService.GetBookByIdAsync(1));
    }

    [Fact]
    public async Task UpdateBookAsync_ShouldUpdateAndReturnDto()
    {
        var book = GetSampleBook();
        var updatedBook = GetSampleBook();
        updatedBook.Name = "Updated Name";

        _bookRepositoryMock.Setup(r => r.GetByIdAsync(book.Id)).ReturnsAsync(book);
        _bookRepositoryMock.Setup(r => r.UpdateAsync(book)).Returns(Task.CompletedTask);

        var result = await _bookService.UpdateBookAsync(book.Id, updatedBook);

        _bookRepositoryMock.Verify(r => r.UpdateAsync(book), Times.Once);
        Assert.Equal("Updated Name", result.Name);
    }

    [Fact]
    public async Task UpdateBookAsync_ShouldThrow_WhenBookNotFound()
    {
        _bookRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Book)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _bookService.UpdateBookAsync(1, GetSampleBook()));
    }

    [Fact]
    public async Task UpdateBookAsync_ShouldThrow_WhenUpdatedBookIsNull()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _bookService.UpdateBookAsync(1, null));
    }

    [Fact]
    public async Task DeleteBookAsync_ShouldDeleteAndReturnTrue_WhenBookExists()
    {
        var book = GetSampleBook();
        _bookRepositoryMock.Setup(r => r.GetByIdAsync(book.Id)).ReturnsAsync(book);
        _bookRepositoryMock.Setup(r => r.DeleteAsync(book)).Returns(Task.CompletedTask);

        var result = await _bookService.DeleteBookAsync(book.Id);

        _bookRepositoryMock.Verify(r => r.DeleteAsync(book), Times.Once);
        Assert.True(result);
    }

    [Fact]
    public async Task DeleteBookAsync_ShouldReturnFalse_WhenBookNotFound()
    {
        _bookRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Book)null);

        var result = await _bookService.DeleteBookAsync(1);

        Assert.False(result);
    }

    private Book GetSampleBook() =>
        new Book
        {
            Id = 1,
            Name = "Sample Book",
            Author = "Author",
            Price = 10.0m,
            Stock = 5,
            PublisherId = 2,
            ReleaseYear = 2020,
            Ean = "1234567890123",
            Isbn = "978-3-16-148410-0",
            Sku = "SKU123",
            CategoryId = 1,

            CreatedAt = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid(),
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = Guid.NewGuid()
        };
}
