using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CartService.Domain.Models;

public class Cart
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int UserId { get; set; }
    public List<CartItem> Items { get; set; } = new List<CartItem>();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
