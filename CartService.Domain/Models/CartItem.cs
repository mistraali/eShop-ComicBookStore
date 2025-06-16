using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CartService.Domain.Models;

public class CartItem
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
