using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvoiceService.Domain.Models;

public class Book : BaseModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(150)]
    public string Name { get; set; }

    [Required]
    [StringLength(100)]
    public string Author { get; set; }

    [Range(0.01, 1000)]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int Stock { get; set; }

    [Required]
    public int PublisherId { get; set; }

    [Range(1900, 2100)]
    public int ReleaseYear { get; set; }

    [StringLength(13, MinimumLength = 13)]
    public string Ean { get; set; }

    [StringLength(13, MinimumLength = 10)]
    public string Isbn { get; set; }

    [StringLength(50)]
    public string Sku { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "CategoryId must be greater than 0")]
    public int CategoryId { get; set; }

}
