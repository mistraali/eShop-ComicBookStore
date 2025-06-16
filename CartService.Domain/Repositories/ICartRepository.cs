using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CartService.Domain.DTOs;
using CartService.Domain.Models;

namespace CartService.Domain.Repositories;

public interface ICartRepository
{
    Task<Cart> CreateCartForUserAsync(Cart cart);

    Task<Cart> GetCartByUserIdAsync(int userId);

    Task<List<Cart>> GetAllCartsAsync();

    Task<CartItem> AddItemToCartAsync(CartItem newitem);

    Task<CartItem> UpdateItemInCartAsync(CartItem newitem);
    
    Task RemoveItemFromCartAsync(int userId, int productId);
   
    Task<Cart> ClearCartByIdAsync(int userId);

    Task<bool> DeleteCartByIdAsync(int userId);
}
