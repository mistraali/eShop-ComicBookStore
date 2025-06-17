using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductService.Domain.DTOs;
using ProductService.Domain.Models;
using ProductService.Domain.Repositories;

namespace ProductService.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IBookRepository _bookRepository;

    public CategoryService(ICategoryRepository categoryRepository, IBookRepository bookRepository)
    {
        _categoryRepository = categoryRepository;
        _bookRepository = bookRepository;
    }

    public async Task<Category> CreateCategoryAsync(Category category)
    {
        if (category == null) throw new ArgumentNullException(nameof(category));
        await _categoryRepository.AddAsync(category);
        return category;
    }

    public async Task<List<Category>> GetAllCategoriesAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();
        return categories.ToList();
    }

    public async Task<Category> GetCategoryByIdAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null) throw new InvalidOperationException("Category not found");
        return category;
    }

    public async Task<List<BookDto>> GetBooksByCategoryAsync(int categoryId)
    {
        var books = await _bookRepository.GetAllAsync();
        var filtered = books.Where(b => b.CategoryId == categoryId);

        return filtered.Select(b => new BookDto
        {
            Id = b.Id,
            Name = b.Name,
            Author = b.Author,
            Price = b.Price,
            Stock = b.Stock,
            PublisherId = b.PublisherId,
            ReleaseYear = b.ReleaseYear,
            Ean = b.Ean,
            Isbn = b.Isbn,
            Sku = b.Sku,
            CategoryId = b.CategoryId
        }).ToList();
    }

    public async Task<Category> UpdateCategoryAsync(int id, Category updatedCategory)
    {
        if (updatedCategory == null) throw new ArgumentNullException(nameof(updatedCategory));
        if (id != updatedCategory.Id) throw new ArgumentException("Id mismatch");

        var existing = await _categoryRepository.GetByIdAsync(id);
        if (existing == null) throw new InvalidOperationException("Category not found");

        existing.Name = updatedCategory.Name;

        await _categoryRepository.UpdateAsync(existing);
        return existing;
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null) return false;

        await _categoryRepository.DeleteAsync(category);
        return true;
    }
}
