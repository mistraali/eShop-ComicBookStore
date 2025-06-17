using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Models;
using ProductService.Domain.Seeders;

namespace ProductService.Domain.Repositories;

public class ProductContext : DbContext
{
    public DbSet<Book> Books { get; set; }
    public DbSet<Category> Categories { get; set; } 

    public ProductContext(DbContextOptions<ProductContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        CategorySeeder.Seed(modelBuilder);
        BookSeeder.Seed(modelBuilder);
    }
}
