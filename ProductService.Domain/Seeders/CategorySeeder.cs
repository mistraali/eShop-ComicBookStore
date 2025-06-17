using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Models;
using System.Collections.Generic;

namespace ProductService.Domain.Seeders
{
    public static class CategorySeeder
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>().HasData(new List<Category>
            {
                new Category
                {
                    Id = 1,
                    Name = "Superhero",
                    Description = "Komiksy z gatunku superbohaterskiego."
                },
                new Category
                {
                    Id = 2,
                    Name = "Manga",
                    Description = "Japońskie komiksy i manga."
                },
                new Category
                {
                    Id = 3,
                    Name = "Fantasy i Science Fiction",
                    Description = "Komiksy fantasy i sci-fi."
                }
            });
        }
    }
}
