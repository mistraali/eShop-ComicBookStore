using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CartService.Domain.Models;

public class CartItem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int CartItemId { get; set; } // Primary key for CartItem
    public int CartId { get; set; } // CartId in Cart == UserId
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
