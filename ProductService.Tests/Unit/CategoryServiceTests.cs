using Moq;
using ProductService.Application.Services;
using ProductService.Domain.Models;
using ProductService.Domain.Repositories;

namespace ProductService.Tests.Unit;
public class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _categoryRepoMock;
    private readonly Mock<IBookRepository> _bookRepoMock;
    private readonly CategoryService _categoryService;

    public CategoryServiceTests()
    {
        _categoryRepoMock = new Mock<ICategoryRepository>();
        _bookRepoMock = new Mock<IBookRepository>();
        _categoryService = new CategoryService(_categoryRepoMock.Object, _bookRepoMock.Object);
    }

    private Category GetSampleCategory(int id = 1) =>
        new Category { Id = id, Name = "Sample Category" };

    private Book GetSampleBook(int id = 1, int categoryId = 1) =>
        new Book
        {
            Id = id,
            Name = "Sample Book",
            Author = "Author",
            Price = 10m,
            Stock = 5,
            PublisherId = 1,
            ReleaseYear = 2020,
            Ean = "1234567890123",
            Isbn = "978-3-16-148410-0",
            Sku = "SKU123",
            CategoryId = categoryId
        };

    [Fact]
    public async Task CreateCategoryAsync_ShouldAddCategory()
    {
        var category = GetSampleCategory();
        _categoryRepoMock.Setup(r => r.AddAsync(category)).Returns(Task.CompletedTask);

        var result = await _categoryService.CreateCategoryAsync(category);

        _categoryRepoMock.Verify(r => r.AddAsync(category), Times.Once);
        Assert.Equal(category, result);
    }

    [Fact]
    public async Task CreateCategoryAsync_ShouldThrow_WhenNull()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _categoryService.CreateCategoryAsync(null));
    }

    [Fact]
    public async Task GetAllCategoriesAsync_ShouldReturnList()
    {
        var categories = new List<Category> { GetSampleCategory(1), GetSampleCategory(2) };
        _categoryRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(categories);

        var result = await _categoryService.GetAllCategoriesAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetCategoryByIdAsync_ShouldReturnCategory_WhenExists()
    {
        var category = GetSampleCategory(1);
        _categoryRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(category);

        var result = await _categoryService.GetCategoryByIdAsync(1);

        Assert.Equal(category, result);
    }

    [Fact]
    public async Task GetCategoryByIdAsync_ShouldThrow_WhenNotFound()
    {
        _categoryRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Category)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _categoryService.GetCategoryByIdAsync(1));
    }

    [Fact]
    public async Task GetBooksByCategoryAsync_ShouldReturnBooksForCategory()
    {
        var books = new List<Book>
        {
            GetSampleBook(1, 1),
            GetSampleBook(2, 2)
        };
        _bookRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(books);

        var result = await _categoryService.GetBooksByCategoryAsync(1);

        Assert.Single(result);
        Assert.Equal(1, result[0].CategoryId);
    }

    [Fact]
    public async Task UpdateCategoryAsync_ShouldUpdateAndReturnCategory()
    {
        var existing = GetSampleCategory(1);
        var updated = new Category { Id = 1, Name = "Updated" };

        _categoryRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);
        _categoryRepoMock.Setup(r => r.UpdateAsync(existing)).Returns(Task.CompletedTask);

        var result = await _categoryService.UpdateCategoryAsync(1, updated);

        _categoryRepoMock.Verify(r => r.UpdateAsync(existing), Times.Once);
        Assert.Equal("Updated", result.Name);
    }

    [Fact]
    public async Task UpdateCategoryAsync_ShouldThrow_WhenIdMismatch()
    {
        var updated = new Category { Id = 2, Name = "Updated" };
        await Assert.ThrowsAsync<ArgumentException>(() => _categoryService.UpdateCategoryAsync(1, updated));
    }

    [Fact]
    public async Task UpdateCategoryAsync_ShouldThrow_WhenNull()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _categoryService.UpdateCategoryAsync(1, null));
    }

    [Fact]
    public async Task UpdateCategoryAsync_ShouldThrow_WhenNotFound()
    {
        _categoryRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Category)null);
        var updated = GetSampleCategory();

        await Assert.ThrowsAsync<InvalidOperationException>(() => _categoryService.UpdateCategoryAsync(1, updated));
    }

    [Fact]
    public async Task DeleteCategoryAsync_ShouldDeleteAndReturnTrue_WhenExists()
    {
        var category = GetSampleCategory(1);
        _categoryRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(category);
        _categoryRepoMock.Setup(r => r.DeleteAsync(category)).Returns(Task.CompletedTask);

        var result = await _categoryService.DeleteCategoryAsync(1);

        _categoryRepoMock.Verify(r => r.DeleteAsync(category), Times.Once);
        Assert.True(result);
    }

    [Fact]
    public async Task DeleteCategoryAsync_ShouldReturnFalse_WhenNotFound()
    {
        _categoryRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Category)null);

        var result = await _categoryService.DeleteCategoryAsync(1);

        Assert.False(result);
    }
}
