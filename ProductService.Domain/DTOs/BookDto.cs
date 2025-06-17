namespace ProductService.Domain.DTOs;

public class BookDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Ean { get; set; }
    public string? Isbn { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string? Sku { get; set; }
    public string Author { get; set; }
    public int PublisherId { get; set; }
    public int ReleaseYear { get; set; }


    public bool Deleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid UpdatedBy { get; set; }
}


