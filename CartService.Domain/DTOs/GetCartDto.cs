using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CartService.Domain.Models;

namespace CartService.Domain.DTOs;

public class GetCartDto
{
    public int UserId { get; set; }
    public List<int> CartItems { get; set; } = new(); // numery Cart Itemów (ProductId)

    public int TotalItems => CartItems.Count; // zwraca ilość różnych produktów a nie ich sumaryczną ilość
}
