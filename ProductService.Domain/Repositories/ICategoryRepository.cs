using ProductService.Domain.Models;

namespace ProductService.Domain.Repositories;

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAllAsync();
    Task<Category?> GetByIdAsync(int id);
    Task<Category?> GetByIdWithBooksAsync(int id);
    Task AddAsync(Category category);
    Task UpdateAsync(Category category);
    Task DeleteAsync(Category category);
    Task<bool> ExistsByNameAsync(string name);
}
