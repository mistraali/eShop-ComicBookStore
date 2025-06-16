using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CartService.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CartService.Domain.Repositories;

public class CartDataContext : DbContext
{
    public CartDataContext(DbContextOptions<CartDataContext> options) : base(options) { }

    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
}
