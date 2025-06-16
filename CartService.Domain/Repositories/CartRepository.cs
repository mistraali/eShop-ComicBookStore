using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CartService.Domain.DTOs;
using CartService.Domain.Models;

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

    Task<Cart> ICartRepository.CreateCartForUserAsync(int userId)
    {
        throw new NotImplementedException();
    }

    Task<Cart> ICartRepository.GetCartByUserIdAsync(int userId)
    {
        throw new NotImplementedException();
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
