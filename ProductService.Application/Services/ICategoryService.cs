using System.Collections.Generic;
using System.Threading.Tasks;
using ProductService.Domain.Models;
using ProductService.Domain.DTOs;

namespace ProductService.Application.Services
{
    public interface ICategoryService
    {
        Task<Category> CreateCategoryAsync(Category category);
        Task<List<Category>> GetAllCategoriesAsync();
        Task<Category> GetCategoryByIdAsync(int id);
        Task<List<BookDto>> GetBooksByCategoryAsync(int categoryId);
        Task<Category> UpdateCategoryAsync(int id, Category updatedCategory);
        Task<bool> DeleteCategoryAsync(int id);
    }
}
