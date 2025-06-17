using Microsoft.AspNetCore.Mvc;
using ProductService.Domain.DTOs;
using ProductService.Domain.Models;
using ProductService.Domain.Repositories;

namespace ProductService.Controllers
{
    [ApiController]
    [Route("api/categories")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IBookRepository _bookRepository;

        public CategoryController(ICategoryRepository categoryRepository, IBookRepository bookRepository)
        {
            _categoryRepository = categoryRepository;
            _bookRepository = bookRepository;
        }


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return Ok(categories);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) return NotFound();
            return Ok(category);
        }

        [HttpGet("{id}/books")]
        public async Task<ActionResult<IEnumerable<BookDto>>> GetBooksByCategory(int id)
        {
            var books = await _bookRepository.GetAllAsync();
            var filtered = books.Where(b => b.CategoryId == id);

            var dtos = filtered.Select(b => new BookDto
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
            });

            return Ok(dtos);
        }


        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Category category)
        {
            await _categoryRepository.AddAsync(category);
            return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Category category)
        {
            if (id != category.Id) return BadRequest();

            var existing = await _categoryRepository.GetByIdAsync(id);
            if (existing == null) return NotFound();

            existing.Name = category.Name;
            await _categoryRepository.UpdateAsync(existing);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) return NotFound();

            await _categoryRepository.DeleteAsync(category);
            return NoContent();
        }
    }
}
