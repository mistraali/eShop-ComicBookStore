using Microsoft.AspNetCore.Mvc;
using ProductService.Application.Services;
using ProductService.Domain.DTOs;
using ProductService.Domain.Models;

namespace ProductService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IBookService _bookService;

    public BooksController(IBookService bookService)
    {
        _bookService = bookService;
    }

    [HttpGet]
    public async Task<ActionResult<List<BookDto>>> GetBooks()
    {
        var books = await _bookService.GetAllBooksAsync();
        return Ok(books);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BookDto>> GetBook(int id)
    {
        try
        {
            var book = await _bookService.GetBookByIdAsync(id);
            return Ok(book);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    public async Task<ActionResult<BookDto>> PostBook(Book book)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var createdBook = await _bookService.CreateBookAsync(book);
        // Zakładam, że CreateBookAsync zwraca Book (w razie potrzeby można mapować do DTO)
        var createdBookDto = new BookDto
        {
            Id = createdBook.Id,
            Name = createdBook.Name,
            Author = createdBook.Author,
            Price = createdBook.Price,
            Stock = createdBook.Stock,
            PublisherId = createdBook.PublisherId,
            ReleaseYear = createdBook.ReleaseYear,
            Ean = createdBook.Ean,
            Isbn = createdBook.Isbn,
            Sku = createdBook.Sku
        };

        return CreatedAtAction(nameof(GetBook), new { id = createdBookDto.Id }, createdBookDto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutBook(int id, Book book)
    {
        if (id != book.Id) return BadRequest();

        try
        {
            var updatedBookDto = await _bookService.UpdateBookAsync(id, book);
            return NoContent();
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBook(int id)
    {
        var success = await _bookService.DeleteBookAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }
}
