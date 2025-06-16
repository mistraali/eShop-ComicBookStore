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

    async Task<CartItem> ICartRepository.AddItemToCartAsync(CartItem newItem)
    {
        await _context.CartItems.AddAsync(newItem);
        await _context.SaveChangesAsync();
        return newItem;
    }

    Task<CartItem> ICartRepository.UpdateItemInCartAsync(CartItem newItem)
    {
        throw new NotImplementedException();
    }

    async Task ICartRepository.ClearCartAsync(int userId)
    {
        throw new NotImplementedException();
    }

    async Task<Cart> ICartRepository.CreateCartForUserAsync(Cart cart)
    {
        await _context.Carts.AddAsync(cart);
        await _context.SaveChangesAsync();
        return cart;
    }

    Task ICartRepository.RemoveItemFromCartAsync(int userId, int productId)
    {
        throw new NotImplementedException();
    }

}
