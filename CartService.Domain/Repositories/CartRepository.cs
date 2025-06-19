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

    public async Task<List<Cart>> GetAllCartsAsync()
    {
        return await _context.Carts.Include(c => c.CartItems).ToListAsync();
    }

    public async Task<Cart> GetCartByUserIdAsync(int userId)
    {
        return await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == userId);
    }

    public async Task<CartItem> AddItemToCartAsync(CartItem newItem)
    {
        await _context.CartItems.AddAsync(newItem);
        await _context.SaveChangesAsync();
        return newItem;
    }

    public async Task<CartItem> UpdateItemInCartAsync(CartItem updatedItem)
    {
        var existingItem = await _context.CartItems
            .FirstOrDefaultAsync(ci => ci.CartItemId == updatedItem.CartItemId);

        if (existingItem == null)
        {
            throw new InvalidOperationException($"CartItem with id {updatedItem.CartItemId} not found.");
        }

        existingItem.Quantity = updatedItem.Quantity;
        // jeśli inne pola też trzeba aktualizować, to tutaj

        await _context.SaveChangesAsync();

        return existingItem;
    }


    public async Task<Cart> ClearCartByIdAsync(int userId)
    {
        var itemsToRemove = await _context.CartItems
            .Where(ci => ci.CartId == userId)
            .ToListAsync();

        _context.CartItems.RemoveRange(itemsToRemove);

        await _context.SaveChangesAsync();
        return await _context.Carts.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.UserId == userId);
    }

    public async Task<Cart> CreateCartForUserAsync(Cart cart)
    {
        await _context.Carts.AddAsync(cart);
        await _context.SaveChangesAsync();
        return cart;
    }

    public async Task<Cart> RemoveItemFromCartAsync(int userId, int productId)
    {

        var itemToRemove = await _context.CartItems
    .FirstAsync(ci => ci.CartId == userId && ci.ProductId == productId);

        _context.CartItems.Remove(itemToRemove);
        await _context.SaveChangesAsync();
        return await _context.Carts.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.UserId == userId);
    }

    public async Task<bool> DeleteCartByIdAsync(int userId)
    {
        var cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId);
        _context.Carts.Remove(cart);
        await _context.SaveChangesAsync();
        return true;
    }
}
