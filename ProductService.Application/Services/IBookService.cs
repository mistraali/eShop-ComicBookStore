using System.Collections.Generic;
using System.Threading.Tasks;
using ProductService.Domain.DTOs;
using ProductService.Domain.Models;

namespace ProductService.Application.Services
{
    public interface IBookService
    {
        Task<BookDto> CreateBookAsync(Book book);
        Task<List<BookDto>> GetAllBooksAsync();
        Task<BookDto> GetBookByIdAsync(int id);
        Task<BookDto> UpdateBookAsync(int id, Book updatedBook);
        Task<bool> DeleteBookAsync(int id);
    }
}
