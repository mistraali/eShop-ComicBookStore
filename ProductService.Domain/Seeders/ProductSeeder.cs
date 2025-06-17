using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Models;
using ProductService.Domain.Repositories;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace ProductService.Domain.Seeders;

public static class ProductSeeder
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>().HasData(new List<Book>
        {
            new Book
            {
                Id = 1,
                Name = "Batman: Year One",
                Author = "Frank Miller",
                Price = 19.99m,
                Stock = 10,
                PublisherId = 1,
                ReleaseYear = 1987,
                Ean = "1234567890123",
                Isbn = "9781401207526",
                Sku = "BATMAN-Y1",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Book
            {
                Id = 2,
                Name = "Dragon Ball Vol. 1",
                Author = "Akira Toriyama",
                Price = 12.99m,
                Stock = 20,
                PublisherId = 3,
                ReleaseYear = 1984,
                Ean = "9781569319208",
                Isbn = "1569319200",
                Sku = "DB-VOL1",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Book
                {
                Id = 3,
                Name = "Thorgal: The Betrayed Sorceress",
                Author = "Jean Van Hamme",
                Price = 29.99m,
                Stock = 12,
                PublisherId = 2,
                ReleaseYear = 1980,
                Ean = "9781595821953",
                Isbn = "1595821955",
                Sku = "THORGAL-VOL1",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        });
    }
}
