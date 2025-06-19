using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CartService.Domain.DTOs;
using CartService.Domain.Models;

namespace CartService.Application.Services;

public interface ICartService
{
    Task<Cart> CreateCartForUserAsync(int userId);
    Task<List<GetCartDto>> GetAllCartsAsync();
    Task<GetCartDto> GetCartByUserIdAsync(int userId);
    Task<GetCartItemDto> AddItemToCartAsync(AddItemToCartDto item);
    Task<bool> DeleteCartByIdAsync(int userId);
    Task<GetCartDto> ClearCartByIdAsync(int userId);

    //Task UpdateItemInCartAsync(int userId, int productId, int quantity);
    Task<GetCartDto> RemoveItemFromCartAsync(int userId, int productId);

    Task<bool> CartCheckoutByIdAsync(int userId);
}
