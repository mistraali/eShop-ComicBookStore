using Microsoft.AspNetCore.Mvc;
using Moq;
using ProductService.Controllers;
using ProductService.Domain.DTOs;
using ProductService.Domain.Models;
using ProductService.Domain.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ProductService.Tests.Unit;
public class CategoryControllerTests
{
    private readonly Mock<ICategoryRepository> _categoryRepoMock;
    private readonly Mock<IBookRepository> _bookRepoMock;
    private readonly CategoryController _controller;

    public CategoryControllerTests()
    {
        _categoryRepoMock = new Mock<ICategoryRepository>();
        _bookRepoMock = new Mock<IBookRepository>();
        _controller = new CategoryController(_categoryRepoMock.Object, _bookRepoMock.Object);
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
    public async Task GetAll_ShouldReturnOkWithCategories()
    {
        var categories = new List<Category> { GetSampleCategory(), GetSampleCategory(2) };
        _categoryRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(categories);

        var result = await _controller.GetAll();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedCategories = Assert.IsAssignableFrom<IEnumerable<Category>>(okResult.Value);
        Assert.Equal(2, returnedCategories.Count());
    }

    [Fact]
    public async Task GetById_ShouldReturnOk_WhenCategoryExists()
    {
        var category = GetSampleCategory(1);
        _categoryRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(category);

        var result = await _controller.GetById(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedCategory = Assert.IsType<Category>(okResult.Value);
        Assert.Equal(category.Id, returnedCategory.Id);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenCategoryDoesNotExist()
    {
        _categoryRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Category)null);

        var result = await _controller.GetById(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetBooksByCategory_ShouldReturnBooksFilteredByCategory()
    {
        var books = new List<Book>
        {
            GetSampleBook(1, 1),
            GetSampleBook(2, 2)
        };
        _bookRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(books);

        var result = await _controller.GetBooksByCategory(1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dtos = Assert.IsAssignableFrom<IEnumerable<BookDto>>(okResult.Value);
        Assert.Single(dtos);
        Assert.All(dtos, b => Assert.Equal(1, b.CategoryId));
    }

    [Fact]
    public async Task Create_ShouldReturnCreatedAtActionResult()
    {
        var category = GetSampleCategory(1);

        _categoryRepoMock.Setup(r => r.AddAsync(category)).Returns(Task.CompletedTask);

        var result = await _controller.Create(category);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(CategoryController.GetById), createdResult.ActionName);
        var returnedCategory = Assert.IsType<Category>(createdResult.Value);
        Assert.Equal(category.Id, returnedCategory.Id);
    }

    [Fact]
    public async Task Update_ShouldReturnNoContent_WhenSuccessful()
    {
        var category = GetSampleCategory(1);

        _categoryRepoMock.Setup(r => r.GetByIdAsync(category.Id)).ReturnsAsync(category);
        _categoryRepoMock.Setup(r => r.UpdateAsync(category)).Returns(Task.CompletedTask);

        var result = await _controller.Update(1, category);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Update_ShouldReturnBadRequest_WhenIdMismatch()
    {
        var category = GetSampleCategory(2);

        var result = await _controller.Update(1, category);

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task Update_ShouldReturnNotFound_WhenCategoryNotFound()
    {
        var category = GetSampleCategory(1);
        _categoryRepoMock.Setup(r => r.GetByIdAsync(category.Id)).ReturnsAsync((Category)null);

        var result = await _controller.Update(1, category);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContent_WhenSuccessful()
    {
        var category = GetSampleCategory(1);
        _categoryRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(category);
        _categoryRepoMock.Setup(r => r.DeleteAsync(category)).Returns(Task.CompletedTask);

        var result = await _controller.Delete(1);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenCategoryNotFound()
    {
        _categoryRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Category)null);

        var result = await _controller.Delete(999);

        Assert.IsType<NotFoundResult>(result);
    }
}
