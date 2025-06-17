using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Models;
using ProductService.Domain.Repositories;

namespace ProductService.Tests.Integration;
public class CategoryRepositoryIntegrationTests
{
    private ProductContext GetContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ProductContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new ProductContext(options);
    }

    private Category CreateCategory(int id = 0, string name = "Category")
    {
        return new Category
        {
            Id = id,
            Name = name,
            Books = new List<Book>()
        };
    }

    [Fact]
    public async Task GetByIdWithBooksAsync_ReturnsCategoryWithBooks()
    {
        var context = GetContext(nameof(GetByIdWithBooksAsync_ReturnsCategoryWithBooks));
        var category = CreateCategory(100, "Comics");

        var book = new Book
        {
            Id = 100,
            Name = "Batman",
            CategoryId = 100,
            Author = "Author Name",
            Price = 20.0m,
            Stock = 5,
            PublisherId = 1,
            ReleaseYear = 2023,
            Ean = "1234567890123",
            Isbn = "978-3-16-148410-0",
            Sku = "SKU100"
        };

        category.Books.Add(book);
        await context.Categories.AddAsync(category);
        await context.SaveChangesAsync();

        var repo = new CategoryRepository(context);
        var result = await repo.GetByIdWithBooksAsync(100);

        Assert.NotNull(result);
        Assert.Equal("Comics", result.Name);
        Assert.NotNull(result.Books);
        Assert.Single(result.Books);
        Assert.Equal("Batman", result.Books.First().Name);
    }


    [Fact]
    public async Task GetByIdWithBooksAsync_ReturnsNull_WhenNotFound()
    {
        var context = GetContext(nameof(GetByIdWithBooksAsync_ReturnsNull_WhenNotFound));
        var repo = new CategoryRepository(context);

        var result = await repo.GetByIdWithBooksAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task AddAsync_ShouldAddCategory()
    {
        var context = GetContext(nameof(AddAsync_ShouldAddCategory));
        var repo = new CategoryRepository(context);
        var category = CreateCategory(name: "New Category");

        await repo.AddAsync(category);

        Assert.Single(context.Categories);
        Assert.Equal("New Category", context.Categories.First().Name);
    }

    [Fact]
    public async Task ExistsByNameAsync_ReturnsTrue_WhenCategoryExists()
    {
        var context = GetContext(nameof(ExistsByNameAsync_ReturnsTrue_WhenCategoryExists));
        var category = CreateCategory(name: "Existing");
        context.Categories.Add(category);
        context.SaveChanges();

        var repo = new CategoryRepository(context);
        var exists = await repo.ExistsByNameAsync("Existing");

        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsByNameAsync_ReturnsFalse_WhenCategoryDoesNotExist()
    {
        var context = GetContext(nameof(ExistsByNameAsync_ReturnsFalse_WhenCategoryDoesNotExist));
        var repo = new CategoryRepository(context);

        var exists = await repo.ExistsByNameAsync("NonExisting");

        Assert.False(exists);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenCategoryNotTracked()
    {
        var context = GetContext(nameof(UpdateAsync_ShouldThrow_WhenCategoryNotTracked));
        var repo = new CategoryRepository(context);
        var category = CreateCategory(999, "NonTracked");

        // Since entity is not tracked and not in DB, updating it causes error
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => repo.UpdateAsync(category));
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrow_WhenCategoryNotTracked()
    {
        var context = GetContext(nameof(DeleteAsync_ShouldThrow_WhenCategoryNotTracked));
        var repo = new CategoryRepository(context);
        var category = CreateCategory(999, "NonTracked");

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => repo.DeleteAsync(category));
    }
}
