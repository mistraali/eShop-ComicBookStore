using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductService.Domain.DTOs;
using ProductService.Domain.Models;
using ProductService.Domain.Repositories;

namespace ProductService.Application.Services;

public class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;

    public BookService(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }

    public async Task<BookDto> CreateBookAsync(Book book)
    {
        if (book == null) throw new ArgumentNullException(nameof(book));
        await _bookRepository.AddAsync(book);
        return MapToDto(book);
    }

    public async Task<List<BookDto>> GetAllBooksAsync()
    {
        var books = await _bookRepository.GetAllAsync();
        return books.Select(MapToDto).ToList();
    }

    public async Task<BookDto> GetBookByIdAsync(int id)
    {
        var book = await _bookRepository.GetByIdAsync(id);
        if (book == null) throw new InvalidOperationException($"Book with id {id} not found.");
        return MapToDto(book);
    }

    public async Task<BookDto> UpdateBookAsync(int id, Book updatedBook)
    {
        if (updatedBook == null) throw new ArgumentNullException(nameof(updatedBook));

        var existingBook = await _bookRepository.GetByIdAsync(id);
        if (existingBook == null) throw new InvalidOperationException($"Book with id {id} not found.");

        existingBook.Name = updatedBook.Name;
        existingBook.Author = updatedBook.Author;
        existingBook.Price = updatedBook.Price;
        existingBook.Stock = updatedBook.Stock;
        existingBook.PublisherId = updatedBook.PublisherId;
        existingBook.ReleaseYear = updatedBook.ReleaseYear;
        existingBook.Ean = updatedBook.Ean;
        existingBook.Isbn = updatedBook.Isbn;
        existingBook.Sku = updatedBook.Sku;

        await _bookRepository.UpdateAsync(existingBook);

        return MapToDto(existingBook);
    }

    public async Task<bool> DeleteBookAsync(int id)
    {
        var book = await _bookRepository.GetByIdAsync(id);
        if (book == null) return false;

        await _bookRepository.DeleteAsync(book);
        return true;
    }

    private static BookDto MapToDto(Book book) =>
        new BookDto
        {
            Id = book.Id,
            Name = book.Name,
            Author = book.Author,
            Price = book.Price,
            Stock = book.Stock,
            PublisherId = book.PublisherId,
            ReleaseYear = book.ReleaseYear,
            Ean = book.Ean,
            Isbn = book.Isbn,
            Sku = book.Sku,
            Deleted = book.Deleted,
            CreatedAt = book.CreatedAt,
            CreatedBy = book.CreatedBy,
            UpdatedAt = book.UpdatedAt,
            UpdatedBy = book.UpdatedBy
        };
}
