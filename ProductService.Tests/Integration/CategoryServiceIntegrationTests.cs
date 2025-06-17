using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProductService.Application.Services;
using ProductService.Domain.Models;
using ProductService.Domain.Repositories;
using Xunit;

namespace ProductService.Tests.Integration;
public class CategoryServiceIntegrationTests
{
    private ProductContext GetInMemoryContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ProductContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new ProductContext(options);
    }

    private Category CreateSampleCategory(int id = 1, string name = "Category 1") =>
        new Category { Id = id, Name = name };

    private Book CreateSampleBook(int id = 1, int categoryId = 1) =>
        new Book
        {
            Id = id,
            Name = "Sample Book",
            Author = "Author",
            Price = 9.99m,
            Stock = 10,
            PublisherId = 1,
            ReleaseYear = 2022,
            Ean = "1234567890123",
            Isbn = "978-3-16-148410-0",
            Sku = "SKU001",
            CategoryId = categoryId
        };

    [Fact]
    public async Task CreateCategoryAsync_ShouldAddCategory()
    {
        using var context = GetInMemoryContext(nameof(CreateCategoryAsync_ShouldAddCategory));
        var categoryRepo = new CategoryRepository(context);
        var bookRepo = new BookRepository(context);
        var service = new CategoryService(categoryRepo, bookRepo);

        var category = CreateSampleCategory();

        var result = await service.CreateCategoryAsync(category);

        Assert.Equal(category, result);
        Assert.Single(await context.Categories.ToListAsync());
    }

    [Fact]
    public async Task CreateCategoryAsync_ShouldThrow_WhenNull()
    {
        using var context = GetInMemoryContext(nameof(CreateCategoryAsync_ShouldThrow_WhenNull));
        var categoryRepo = new CategoryRepository(context);
        var bookRepo = new BookRepository(context);
        var service = new CategoryService(categoryRepo, bookRepo);

        await Assert.ThrowsAsync<ArgumentNullException>(() => service.CreateCategoryAsync(null));
    }

    [Fact]
    public async Task GetAllCategoriesAsync_ShouldReturnAllCategories()
    {
        using var context = GetInMemoryContext(nameof(GetAllCategoriesAsync_ShouldReturnAllCategories));
        context.Categories.AddRange(CreateSampleCategory(1), CreateSampleCategory(2));
        await context.SaveChangesAsync();

        var categoryRepo = new CategoryRepository(context);
        var bookRepo = new BookRepository(context);
        var service = new CategoryService(categoryRepo, bookRepo);

        var categories = await service.GetAllCategoriesAsync();

        Assert.Equal(2, categories.Count);
    }

    [Fact]
    public async Task GetCategoryByIdAsync_ShouldReturnCategory_WhenExists()
    {
        using var context = GetInMemoryContext(nameof(GetCategoryByIdAsync_ShouldReturnCategory_WhenExists));
        var category = CreateSampleCategory(1);
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var categoryRepo = new CategoryRepository(context);
        var bookRepo = new BookRepository(context);
        var service = new CategoryService(categoryRepo, bookRepo);

        var result = await service.GetCategoryByIdAsync(1);

        Assert.Equal(category, result);
    }

    [Fact]
    public async Task GetCategoryByIdAsync_ShouldThrow_WhenNotFound()
    {
        using var context = GetInMemoryContext(nameof(GetCategoryByIdAsync_ShouldThrow_WhenNotFound));
        var categoryRepo = new CategoryRepository(context);
        var bookRepo = new BookRepository(context);
        var service = new CategoryService(categoryRepo, bookRepo);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetCategoryByIdAsync(999));
    }

    [Fact]
    public async Task GetBooksByCategoryAsync_ShouldReturnBooksForCategory()
    {
        using var context = GetInMemoryContext(nameof(GetBooksByCategoryAsync_ShouldReturnBooksForCategory));
        var book1 = CreateSampleBook(1, categoryId: 1);
        var book2 = CreateSampleBook(2, categoryId: 2);
        context.Books.AddRange(book1, book2);
        await context.SaveChangesAsync();

        var categoryRepo = new CategoryRepository(context);
        var bookRepo = new BookRepository(context);
        var service = new CategoryService(categoryRepo, bookRepo);

        var books = await service.GetBooksByCategoryAsync(1);

        Assert.Single(books);
        Assert.Equal(1, books[0].CategoryId);
    }

    [Fact]
    public async Task UpdateCategoryAsync_ShouldUpdateCategory_WhenValid()
    {
        using var context = GetInMemoryContext(nameof(UpdateCategoryAsync_ShouldUpdateCategory_WhenValid));
        var existing = CreateSampleCategory(1, "OldName");
        context.Categories.Add(existing);
        await context.SaveChangesAsync();

        var categoryRepo = new CategoryRepository(context);
        var bookRepo = new BookRepository(context);
        var service = new CategoryService(categoryRepo, bookRepo);

        var updatedCategory = new Category { Id = 1, Name = "NewName" };
        var result = await service.UpdateCategoryAsync(1, updatedCategory);

        Assert.Equal("NewName", result.Name);
    }

    [Fact]
    public async Task UpdateCategoryAsync_ShouldThrow_WhenIdMismatch()
    {
        using var context = GetInMemoryContext(nameof(UpdateCategoryAsync_ShouldThrow_WhenIdMismatch));
        var categoryRepo = new CategoryRepository(context);
        var bookRepo = new BookRepository(context);
        var service = new CategoryService(categoryRepo, bookRepo);

        var updatedCategory = new Category { Id = 2, Name = "NewName" };

        await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateCategoryAsync(1, updatedCategory));
    }

    [Fact]
    public async Task UpdateCategoryAsync_ShouldThrow_WhenNull()
    {
        using var context = GetInMemoryContext(nameof(UpdateCategoryAsync_ShouldThrow_WhenNull));
        var categoryRepo = new CategoryRepository(context);
        var bookRepo = new BookRepository(context);
        var service = new CategoryService(categoryRepo, bookRepo);

        await Assert.ThrowsAsync<ArgumentNullException>(() => service.UpdateCategoryAsync(1, null));
    }

    [Fact]
    public async Task UpdateCategoryAsync_ShouldThrow_WhenNotFound()
    {
        using var context = GetInMemoryContext(nameof(UpdateCategoryAsync_ShouldThrow_WhenNotFound));
        var categoryRepo = new CategoryRepository(context);
        var bookRepo = new BookRepository(context);
        var service = new CategoryService(categoryRepo, bookRepo);

        var updatedCategory = CreateSampleCategory(1);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateCategoryAsync(1, updatedCategory));
    }

    [Fact]
    public async Task DeleteCategoryAsync_ShouldDeleteCategory_WhenExists()
    {
        using var context = GetInMemoryContext(nameof(DeleteCategoryAsync_ShouldDeleteCategory_WhenExists));
        var category = CreateSampleCategory(1);
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var categoryRepo = new CategoryRepository(context);
        var bookRepo = new BookRepository(context);
        var service = new CategoryService(categoryRepo, bookRepo);

        var result = await service.DeleteCategoryAsync(1);

        Assert.True(result);
        Assert.Empty(await context.Categories.ToListAsync());
    }

    [Fact]
    public async Task DeleteCategoryAsync_ShouldReturnFalse_WhenNotFound()
    {
        using var context = GetInMemoryContext(nameof(DeleteCategoryAsync_ShouldReturnFalse_WhenNotFound));
        var categoryRepo = new CategoryRepository(context);
        var bookRepo = new BookRepository(context);
        var service = new CategoryService(categoryRepo, bookRepo);

        var result = await service.DeleteCategoryAsync(999);

        Assert.False(result);
    }
}
