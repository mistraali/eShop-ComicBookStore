using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductService.Application.Services;
using ProductService.Controllers;
using ProductService.Domain.DTOs;
using ProductService.Domain.Models;
using ProductService.Domain.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ProductService.Tests.Integration
{
    public class BooksControllerIntegrationTests
    {
        private readonly DbContextOptions<ProductContext> _dbContextOptions;

        public BooksControllerIntegrationTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<ProductContext>()
                .UseInMemoryDatabase($"BookControllerTests_{System.Guid.NewGuid()}")
                .Options;
        }

        private class TestProductContext : ProductContext
        {
            public TestProductContext(DbContextOptions<ProductContext> options) : base(options) { }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                // Pomijamy seedy
            }
        }

        private async Task<BooksController> CreateControllerWithSeedDataAsync()
        {
            var context = new TestProductContext(_dbContextOptions);
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();

            var category1 = new Category { Name = "Category 1" };
            var category2 = new Category { Name = "Category 2" };
            context.Categories.AddRange(category1, category2);
            await context.SaveChangesAsync();

            context.Books.AddRange(new List<Book>
            {
                new Book
                {
                    Name = "Book One",
                    Author = "Author A",
                    Price = 10,
                    Stock = 5,
                    PublisherId = 1,
                    ReleaseYear = 2020,
                    CategoryId = category1.Id,
                    Ean = "1111111111111",
                    Isbn = "9781111111111",
                    Sku = "SKU001"
                },
                new Book
                {
                    Name = "Book Two",
                    Author = "Author B",
                    Price = 15,
                    Stock = 3,
                    PublisherId = 2,
                    ReleaseYear = 2021,
                    CategoryId = category2.Id,
                    Ean = "2222222222222",
                    Isbn = "9782222222222",
                    Sku = "SKU002"
                }
            });

            await context.SaveChangesAsync();

            var repo = new BookRepository(context);
            var service = new BookService(repo);
            return new BooksController(service);
        }

        [Fact]
        public async Task GetBooks_ReturnsAllBooks()
        {
            var controller = await CreateControllerWithSeedDataAsync();

            var result = await controller.GetBooks();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var books = Assert.IsType<List<BookDto>>(okResult.Value);
            Assert.Equal(2, books.Count);
        }

        [Fact]
        public async Task GetBook_ReturnsCorrectBook()
        {
            var controller = await CreateControllerWithSeedDataAsync();

            var getAllResult = await controller.GetBooks();
            var okListResult = Assert.IsType<OkObjectResult>(getAllResult.Result);
            var allBooks = Assert.IsType<List<BookDto>>(okListResult.Value);

            var expectedBook = allBooks.First(b => b.Name == "Book One");

            var result = await controller.GetBook(expectedBook.Id);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var book = Assert.IsType<BookDto>(okResult.Value);
            Assert.Equal("Book One", book.Name);
        }

        [Fact]
        public async Task PostBook_CreatesBook()
        {
            var controller = await CreateControllerWithSeedDataAsync();

            var book = new Book
            {
                Name = "New Book",
                Author = "New Author",
                Price = 20,
                Stock = 10,
                PublisherId = 3,
                ReleaseYear = 2022,
                Ean = "3333333333333",
                Isbn = "9783333333333",
                Sku = "SKU003",
                CategoryId = 1
            };

            var result = await controller.PostBook(book);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var createdBook = Assert.IsType<BookDto>(createdResult.Value);
            Assert.Equal("New Book", createdBook.Name);
        }

        [Fact]
        public async Task PutBook_UpdatesBook()
        {
            var controller = await CreateControllerWithSeedDataAsync();

            var getAllResult = await controller.GetBooks();
            var okListResult = Assert.IsType<OkObjectResult>(getAllResult.Result);
            var allBooks = Assert.IsType<List<BookDto>>(okListResult.Value);

            var bookToUpdate = allBooks.First(b => b.Name == "Book One");

            var updatedBook = new Book
            {
                Id = bookToUpdate.Id,
                Name = "Updated Name",
                Author = "Updated Author",
                Price = 12,
                Stock = 6,
                PublisherId = 1,
                ReleaseYear = 2020,
                Ean = "1111111111111",
                Isbn = "9781111111111",
                Sku = "SKU001",
                CategoryId = bookToUpdate.CategoryId
            };

            var result = await controller.PutBook(bookToUpdate.Id, updatedBook);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteBook_DeletesBook()
        {
            var controller = await CreateControllerWithSeedDataAsync();

            var getAllResult = await controller.GetBooks();
            var okListResult = Assert.IsType<OkObjectResult>(getAllResult.Result);
            var allBooks = Assert.IsType<List<BookDto>>(okListResult.Value);

            var bookToDelete = allBooks.First(b => b.Name == "Book One");

            var result = await controller.DeleteBook(bookToDelete.Id);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteBook_ReturnsNotFound_WhenBookNotExists()
        {
            var controller = await CreateControllerWithSeedDataAsync();

            var result = await controller.DeleteBook(999);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}
