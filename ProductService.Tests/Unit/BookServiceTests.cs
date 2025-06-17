using Moq;
using ProductService.Application.Services;
using ProductService.Domain.Models;
using ProductService.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ProductService.Tests.Unit;
public class BookServiceTests
{
    private readonly Mock<IBookRepository> _bookRepositoryMock;
    private readonly BookService _bookService;

    public BookServiceTests()
    {
        _bookRepositoryMock = new Mock<IBookRepository>();
        _bookService = new BookService(_bookRepositoryMock.Object);
    }

    private Book GetSampleBook(int id = 1) =>
        new Book
        {
            Id = id,
            Name = "Sample Book",
            Author = "Author",
            Price = 10.0m,
            Stock = 5,
            PublisherId = 1,
            ReleaseYear = 2020,
            Ean = "1234567890123",
            Isbn = "978-3-16-148410-0",
            Sku = "SKU123",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid(),
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = Guid.NewGuid()
        };

    [Fact]
    public async Task CreateBookAsync_ShouldAddBookAndReturnDto()
    {
        var book = GetSampleBook();

        _bookRepositoryMock.Setup(r => r.AddAsync(book)).Returns(Task.CompletedTask);

        var result = await _bookService.CreateBookAsync(book);

        _bookRepositoryMock.Verify(r => r.AddAsync(book), Times.Once);
        Assert.Equal(book.Id, result.Id);
        Assert.Equal(book.Name, result.Name);
    }

    [Fact]
    public async Task CreateBookAsync_ShouldThrowArgumentNullException_WhenBookIsNull()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _bookService.CreateBookAsync(null));
    }

    [Fact]
    public async Task GetAllBooksAsync_ShouldReturnAllBooksMappedToDto()
    {
        var books = new List<Book> { GetSampleBook(1), GetSampleBook(2) };

        _bookRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(books);

        var result = await _bookService.GetAllBooksAsync();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, b => b.Id == 1);
        Assert.Contains(result, b => b.Id == 2);
    }

    [Fact]
    public async Task GetBookByIdAsync_ShouldReturnBookDto_WhenBookExists()
    {
        var book = GetSampleBook();

        _bookRepositoryMock.Setup(r => r.GetByIdAsync(book.Id)).ReturnsAsync(book);

        var result = await _bookService.GetBookByIdAsync(book.Id);

        Assert.NotNull(result);
        Assert.Equal(book.Id, result.Id);
    }

    [Fact]
    public async Task GetBookByIdAsync_ShouldThrowInvalidOperationException_WhenBookDoesNotExist()
    {
        _bookRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Book)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _bookService.GetBookByIdAsync(999));
    }

    [Fact]
    public async Task UpdateBookAsync_ShouldUpdateBookAndReturnDto()
    {
        var existingBook = GetSampleBook(1);
        var updatedBook = GetSampleBook(1);
        updatedBook.Name = "Updated Name";

        _bookRepositoryMock.Setup(r => r.GetByIdAsync(existingBook.Id)).ReturnsAsync(existingBook);
        _bookRepositoryMock.Setup(r => r.UpdateAsync(existingBook)).Returns(Task.CompletedTask);

        var result = await _bookService.UpdateBookAsync(existingBook.Id, updatedBook);

        _bookRepositoryMock.Verify(r => r.UpdateAsync(existingBook), Times.Once);
        Assert.Equal("Updated Name", result.Name);
    }

    [Fact]
    public async Task UpdateBookAsync_ShouldThrowArgumentNullException_WhenUpdatedBookIsNull()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _bookService.UpdateBookAsync(1, null));
    }

    [Fact]
    public async Task UpdateBookAsync_ShouldThrowInvalidOperationException_WhenBookNotFound()
    {
        _bookRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Book)null);

        var updatedBook = GetSampleBook();

        await Assert.ThrowsAsync<InvalidOperationException>(() => _bookService.UpdateBookAsync(1, updatedBook));
    }

    [Fact]
    public async Task DeleteBookAsync_ShouldDeleteBookAndReturnTrue_WhenBookExists()
    {
        var book = GetSampleBook();

        _bookRepositoryMock.Setup(r => r.GetByIdAsync(book.Id)).ReturnsAsync(book);
        _bookRepositoryMock.Setup(r => r.DeleteAsync(book)).Returns(Task.CompletedTask);

        var result = await _bookService.DeleteBookAsync(book.Id);

        _bookRepositoryMock.Verify(r => r.DeleteAsync(book), Times.Once);
        Assert.True(result);
    }

    [Fact]
    public async Task DeleteBookAsync_ShouldReturnFalse_WhenBookDoesNotExist()
    {
        _bookRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Book)null);

        var result = await _bookService.DeleteBookAsync(999);

        Assert.False(result);
    }
}
