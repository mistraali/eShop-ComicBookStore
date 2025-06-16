using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

    async Task<Cart> ICartService.CreateCartForUserAsync(int userId)
    {
        var cart = new Cart { UserId = userId };

        var result = await _cartRepository.CreateCartForUserAsync(cart);
        return result;
    }
}
