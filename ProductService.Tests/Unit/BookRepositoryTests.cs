using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Repositories;
using ProductService.Domain.Models;

namespace ProductService.Tests.Unit
{
    public class BookRepositoryTests
    {
        private ProductContext GetContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ProductContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new ProductContext(options);
        }

        [Fact]
        public async Task AddAsync_AddsBookToDb()
        {
            var context = GetContext(nameof(AddAsync_AddsBookToDb));
            var repo = new BookRepository(context);

            var book = new Book { Name = "Test", Author = "Author", Price = 10, PublisherId = 1, ReleaseYear = 2020 };

            await repo.AddAsync(book);

            Assert.Single(context.Books);
            Assert.Equal("Test", context.Books.First().Name);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllBooks()
        {
            var context = GetContext(nameof(GetAllAsync_ReturnsAllBooks));
            context.Books.AddRange(
                new Book { Name = "Book1", Author = "A", Price = 1, PublisherId = 1, ReleaseYear = 2000 },
                new Book { Name = "Book2", Author = "B", Price = 2, PublisherId = 2, ReleaseYear = 2010 }
            );
            context.SaveChanges();

            var repo = new BookRepository(context);
            var result = await repo.GetAllAsync();

            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsCorrectBook()
        {
            var context = GetContext(nameof(GetByIdAsync_ReturnsCorrectBook));
            var book = new Book { Id = 123, Name = "FindMe", Author = "Author", Price = 10, PublisherId = 1, ReleaseYear = 2020 };
            context.Books.Add(book);
            context.SaveChanges();

            var repo = new BookRepository(context);
            var result = await repo.GetByIdAsync(123);

            Assert.NotNull(result);
            Assert.Equal("FindMe", result?.Name);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesBook()
        {
            var context = GetContext(nameof(UpdateAsync_UpdatesBook));
            var book = new Book { Id = 1, Name = "Old", Author = "A", Price = 5, PublisherId = 1, ReleaseYear = 2000 };
            context.Books.Add(book);
            context.SaveChanges();

            var repo = new BookRepository(context);
            book.Name = "Updated";
            await repo.UpdateAsync(book);

            var updated = await context.Books.FindAsync(1);
            Assert.Equal("Updated", updated?.Name);
        }

        [Fact]
        public async Task DeleteAsync_RemovesBook()
        {
            var context = GetContext(nameof(DeleteAsync_RemovesBook));
            var book = new Book { Id = 2, Name = "ToDelete", Author = "A", Price = 5, PublisherId = 1, ReleaseYear = 2000 };
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
    }
}
