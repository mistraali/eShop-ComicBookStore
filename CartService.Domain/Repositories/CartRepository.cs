using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CartService.Domain.DTOs;
using CartService.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CartService.Domain.Repositories;

public class CartRepository : ICartRepository
{
    private readonly CartDataContext _context;
    public CartRepository(CartDataContext context)
    {
        _context = context;
    }

    Task<CartItem> ICartRepository.AddItemToCartAsync(int userId, AddItemToCartDto addItemToCartDto)
    {
        throw new NotImplementedException();
    }

    Task ICartRepository.ClearCartAsync(int userId)
    {
        throw new NotImplementedException();
    }

    async Task<Cart> ICartRepository.CreateCartForUserAsync(Cart cart)
    {
        _context.Carts.Add(cart);
        await _context.SaveChangesAsync();
        return cart;
    }

    async Task<List<Cart>> ICartRepository.GetAllCartsAsync()
    {
        return await _context.Carts.Include(c => c.CartItems).ToListAsync();
    }

    async Task<Cart> ICartRepository.GetCartByUserIdAsync(int userId)
    {
        return await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == userId);
    }

    Task ICartRepository.RemoveItemFromCartAsync(int userId, int productId)
    {
        throw new NotImplementedException();
    }

    Task<CartItem> ICartRepository.UpdateItemInCartAsync(int userId, AddItemToCartDto addItemToCartDto)
    {
        throw new NotImplementedException();
    }
}
