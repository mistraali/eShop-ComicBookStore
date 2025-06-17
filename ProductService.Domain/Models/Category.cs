using System.ComponentModel.DataAnnotations;

namespace ProductService.Domain.Models;

public class Category : BaseModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [StringLength(250)]
    public string? Description { get; set; }

    public ICollection<Book> Books { get; set; } = new List<Book>();
}
