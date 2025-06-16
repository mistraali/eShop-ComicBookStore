using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CartService.Domain.DTOs;
using CartService.Domain.Models;
using CartService.Domain.Repositories;

namespace CartService.Application.Services;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    public CartService(ICartRepository cartRepository)
    {
        _cartRepository = cartRepository;
    }

    public async Task<Cart> CreateCartForUserAsync(int userId)
    {
        var cart = new Cart { UserId = userId };

        var result = await _cartRepository.CreateCartForUserAsync(cart);
        return result;
    }

    public async Task<List<GetCartDto>> GetAllCartsAsync()
    {
        var carts =  await _cartRepository.GetAllCartsAsync();

        var cartsDtos = carts.Select (carts => new GetCartDto
        {
            UserId = carts.UserId,
            CartItems = carts.CartItems.Select(c => c.ProductId).ToList()
        }).ToList();

        return cartsDtos;
    }

    public async Task<GetCartDto> GetCartByUserIdAsync(int userId)
    {
        var cart = await _cartRepository.GetCartByUserIdAsync(userId);

        if (cart == null)
        {
            throw new InvalidOperationException($"Cart for user with id: {userId} does not exist.");
        }
        
        var cartDto = new GetCartDto
        {
            UserId = cart.UserId,
            CartItems = cart.CartItems.Select(c => c.ProductId).ToList()
        };
        return cartDto;
    }

    public async Task<GetCartItemDto> AddItemToCartAsync(AddItemToCartDto item)
    {
        var newItem = new CartItem
        {
            CartId = item.CartId,
            ProductId = item.ProductId,
            Quantity = item.Quantity
        };

        var result = await _cartRepository.AddItemToCartAsync(newItem);  
        
        var dto = new GetCartItemDto
        {
            CartItemId = result.CartItemId,
            CartId = result.CartId,
            ProductId = result.ProductId,
            Quantity = result.Quantity
        };

        return dto;
    }
}
