using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductService.Domain.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ProductContext _context;

        public CategoryRepository(ProductContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _context.Categories.ToListAsync();
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _context.Categories.FindAsync(id);
        }

        public async Task<Category?> GetByIdWithBooksAsync(int id)
        {
            return await _context.Categories
                .Include(c => c.Books)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task AddAsync(Category category)
        {
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Category category)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Category category)
        {
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await _context.Categories.AnyAsync(c => c.Name == name);
        }
    }
}

