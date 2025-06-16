using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Models;

namespace ProductService.Domain.Repositories;

public class ProductContext : DbContext
{
        public DbSet<Book> Books { get; set; }

        public ProductContext(DbContextOptions<ProductContext> options) : base(options) { }
}



