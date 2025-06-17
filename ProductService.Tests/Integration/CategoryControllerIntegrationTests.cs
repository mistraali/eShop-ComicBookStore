using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductService.Controllers;
using ProductService.Domain.DTOs;
using ProductService.Domain.Models;
using ProductService.Domain.Repositories;
using Xunit;

namespace ProductService.Tests.Integration;

public class CategoryControllerIntegrationTests
{
    private ProductContext GetInMemoryContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ProductContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new ProductContext(options);
    }

    [Fact]
    public async Task GetAll_ReturnsAllCategories()
    {
        using var context = GetInMemoryContext("GetAllCategories");
        context.Categories.AddRange(
            new Category { Id = 1, Name = "Cat1" },
            new Category { Id = 2, Name = "Cat2" }
        );
        await context.SaveChangesAsync();

        var controller = new CategoryController(new CategoryRepository(context), new BookRepository(context));

        var result = await controller.GetAll();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var categories = Assert.IsAssignableFrom<IEnumerable<Category>>(okResult.Value);

        Assert.Equal(2, categories.Count());
    }

    [Fact]
    public async Task GetById_ReturnsCategory_WhenExists()
    {
        using var context = GetInMemoryContext("GetByIdCategory");
        var category = new Category { Id = 10, Name = "Category10" };
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var controller = new CategoryController(new CategoryRepository(context), new BookRepository(context));

        var result = await controller.GetById(10);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedCategory = Assert.IsType<Category>(okResult.Value);

        Assert.Equal(10, returnedCategory.Id);
        Assert.Equal("Category10", returnedCategory.Name);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenCategoryMissing()
    {
        using var context = GetInMemoryContext("GetByIdCategory_NotFound");
        var controller = new CategoryController(new CategoryRepository(context), new BookRepository(context));

        var result = await controller.GetById(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetBooksByCategory_ReturnsBooksMappedToDto()
    {
        using var context = GetInMemoryContext("GetBooksByCategory");
        var category = new Category { Id = 5, Name = "BooksCategory" };
        context.Categories.Add(category);

        var books = new List<Book>
        {
            new Book
            {
                Id = 1,
                Name = "Book1",
                Author = "Author1",
                Price = 10,
                Stock = 5,
                PublisherId = 1,
                ReleaseYear = 2021,
                Ean = "1111111111111",
                Isbn = "2222222222",
                Sku = "SKU1",
                CategoryId = 5,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = Guid.NewGuid(),
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = Guid.NewGuid()
            },
            new Book
            {
                Id = 2,
                Name = "Book2",
                Author = "Author2",
                Price = 15,
                Stock = 10,
                PublisherId = 2,
                ReleaseYear = 2020,
                Ean = "3333333333333",
                Isbn = "4444444444",
                Sku = "SKU2",
                CategoryId = 5,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = Guid.NewGuid(),
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = Guid.NewGuid()
            },
            new Book
            {
                Id = 3,
                Name = "OtherBook",
                Author = "Author3",
                Price = 20,
                Stock = 7,
                PublisherId = 3,
                ReleaseYear = 2019,
                Ean = "5555555555555",
                Isbn = "6666666666",
                Sku = "SKU3",
                CategoryId = 99,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = Guid.NewGuid(),
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = Guid.NewGuid()
            }
        };

        context.Books.AddRange(books);
        await context.SaveChangesAsync();

        var controller = new CategoryController(new CategoryRepository(context), new BookRepository(context));

        var actionResult = await controller.GetBooksByCategory(5);

        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var dtos = Assert.IsAssignableFrom<IEnumerable<BookDto>>(okResult.Value);

        // Sprawdź, że zwrócone tylko książki o CategoryId = 5
        Assert.Equal(2, dtos.Count());
        Assert.All(dtos, b => Assert.Equal(5, b.CategoryId));
    }

    [Fact]
    public async Task Create_AddsCategory_ReturnsCreatedAtAction()
    {
        using var context = GetInMemoryContext("CreateCategory");
        var controller = new CategoryController(new CategoryRepository(context), new BookRepository(context));

        var category = new Category { Id = 1, Name = "NewCategory" };

        var result = await controller.Create(category);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var returnedCategory = Assert.IsType<Category>(createdResult.Value);

        Assert.Equal("NewCategory", returnedCategory.Name);

        // Sprawdź, że faktycznie w bazie jest zapisane
        var savedCategory = await context.Categories.FindAsync(1);
        Assert.NotNull(savedCategory);
        Assert.Equal("NewCategory", savedCategory.Name);
    }

    [Fact]
    public async Task Update_ValidId_UpdatesCategory_ReturnsNoContent()
    {
        using var context = GetInMemoryContext("UpdateCategory");
        var category = new Category { Id = 1, Name = "OldName" };
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var controller = new CategoryController(new CategoryRepository(context), new BookRepository(context));
        var updatedCategory = new Category { Id = 1, Name = "UpdatedName" };

        var result = await controller.Update(1, updatedCategory);

        Assert.IsType<NoContentResult>(result);

        var saved = await context.Categories.FindAsync(1);
        Assert.Equal("UpdatedName", saved.Name);
    }

    [Fact]
    public async Task Update_ReturnsBadRequest_WhenIdMismatch()
    {
        using var context = GetInMemoryContext("UpdateBadRequest");
        var controller = new CategoryController(new CategoryRepository(context), new BookRepository(context));
        var category = new Category { Id = 1, Name = "Name" };

        var result = await controller.Update(2, category); // id 2 vs category.Id 1

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenCategoryDoesNotExist()
    {
        using var context = GetInMemoryContext("UpdateNotFound");
        var controller = new CategoryController(new CategoryRepository(context), new BookRepository(context));
        var category = new Category { Id = 1, Name = "Name" };

        var result = await controller.Update(1, category);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_ExistingCategory_ReturnsNoContent()
    {
        using var context = GetInMemoryContext("DeleteCategory");
        var category = new Category { Id = 1, Name = "ToDelete" };
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var controller = new CategoryController(new CategoryRepository(context), new BookRepository(context));

        var result = await controller.Delete(1);

        Assert.IsType<NoContentResult>(result);

        var deleted = await context.Categories.FindAsync(1);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenCategoryMissing()
    {
        using var context = GetInMemoryContext("DeleteNotFound");
        var controller = new CategoryController(new CategoryRepository(context), new BookRepository(context));

        var result = await controller.Delete(999);

        Assert.IsType<NotFoundResult>(result);
    }
}
