using CartService.Application.Infrastructure.Services;
using CartService.Application.Services;
using CartService.Domain.DTOs;
using CartService.Domain.Models;
using CartService.Domain.Repositories;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace CartService.Tests.Unit;
public class CartServiceTests
{
    private readonly Mock<ICartRepository> _cartRepositoryMock;
    private readonly Mock<IProductServiceClient> _productServiceClientMock;
    private readonly CartService.Application.Services.CartService _cartService;

    public CartServiceTests()
    {
        _cartRepositoryMock = new Mock<ICartRepository>();
        _productServiceClientMock = new Mock<IProductServiceClient>();
        _productServiceClientMock
            .Setup(p => p.CheckIfProductExistsAsync(It.IsAny<int>()))
            .ReturnsAsync(true); // domyślnie produkt istnieje

        _cartService = new CartService.Application.Services.CartService(
            _cartRepositoryMock.Object,
            _productServiceClientMock.Object
        );
    }

    [Fact]
    public async Task CreateCartForUserAsync_CreatesCart()
    {
        var userId = 1;
        var cart = new Cart { UserId = userId };

        _cartRepositoryMock.Setup(r => r.CreateCartForUserAsync(It.IsAny<Cart>()))
            .ReturnsAsync(cart);

        var result = await _cartService.CreateCartForUserAsync(userId);

        Assert.Equal(userId, result.UserId);
        _cartRepositoryMock.Verify(r => r.CreateCartForUserAsync(It.Is<Cart>(c => c.UserId == userId)), Times.Once);
    }

    [Fact]
    public async Task GetAllCartsAsync_ReturnsListOfCartDtos()
    {
        var carts = new List<Cart>
        {
            new Cart { UserId = 1, CartItems = new List<CartItem>{ new CartItem { ProductId = 10 } } },
            new Cart { UserId = 2, CartItems = new List<CartItem>{ new CartItem { ProductId = 20 } } }
        };

        _cartRepositoryMock.Setup(r => r.GetAllCartsAsync()).ReturnsAsync(carts);

        var result = await _cartService.GetAllCartsAsync();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, c => c.UserId == 1 && c.CartItems.Contains(10));
        Assert.Contains(result, c => c.UserId == 2 && c.CartItems.Contains(20));
    }

    [Fact]
    public async Task GetCartByUserIdAsync_ExistingCart_ReturnsCartDto()
    {
        var userId = 1;
        var cart = new Cart { UserId = userId, CartItems = new List<CartItem> { new CartItem { ProductId = 100 } } };

        _cartRepositoryMock.Setup(r => r.GetCartByUserIdAsync(userId)).ReturnsAsync(cart);

        var result = await _cartService.GetCartByUserIdAsync(userId);

        Assert.Equal(userId, result.UserId);
        Assert.Contains(100, result.CartItems);
    }

    [Fact]
    public async Task GetCartByUserIdAsync_NonExistingCart_Throws()
    {
        var userId = 1;

        _cartRepositoryMock.Setup(r => r.GetCartByUserIdAsync(userId)).ReturnsAsync((Cart)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _cartService.GetCartByUserIdAsync(userId));
    }

    [Fact]
    public async Task AddItemToCartAsync_AddsAndReturnsDto()
    {
        var dto = new AddItemToCartDto { CartId = 1, ProductId = 2, Quantity = 3 };
        var cartItem = new CartItem { CartItemId = 5, CartId = dto.CartId, ProductId = dto.ProductId, Quantity = dto.Quantity };

        _cartRepositoryMock.Setup(r => r.AddItemToCartAsync(It.IsAny<CartItem>())).ReturnsAsync(cartItem);

        var result = await _cartService.AddItemToCartAsync(dto);

        Assert.Equal(cartItem.CartItemId, result.CartItemId);
        Assert.Equal(cartItem.CartId, result.CartId);
        Assert.Equal(cartItem.ProductId, result.ProductId);
        Assert.Equal(cartItem.Quantity, result.Quantity);
    }

    [Fact]
    public async Task RemoveItemFromCartAsync_ExistingItem_RemovesAndReturnsUpdatedDto()
    {
        var userId = 1;
        var productId = 10;

        var cart = new Cart
        {
            UserId = userId,
            CartItems = new List<CartItem>
        {
            new CartItem { ProductId = productId },
            new CartItem { ProductId = 99 }
        }
        };

        var cartInMemory = cart;

        _cartRepositoryMock.Setup(r => r.GetCartByUserIdAsync(userId))
            .ReturnsAsync(() => cartInMemory);

        _cartRepositoryMock.Setup(r => r.RemoveItemFromCartAsync(userId, productId))
            .ReturnsAsync(() =>
            {
                cartInMemory.CartItems.RemoveAll(ci => ci.ProductId == productId);
                return cartInMemory;
            });

        var result = await _cartService.RemoveItemFromCartAsync(userId, productId);

        Assert.Equal(userId, result.UserId);
        Assert.DoesNotContain(result.CartItems, ci => ci == productId);
        Assert.Contains(result.CartItems, ci => ci == 99);
    }




    [Fact]
    public async Task RemoveItemFromCartAsync_NonExistingCart_Throws()
    {
        var userId = 1;
        var productId = 2;

        _cartRepositoryMock.Setup(r => r.GetCartByUserIdAsync(userId)).ReturnsAsync((Cart)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _cartService.RemoveItemFromCartAsync(userId, productId));
    }

    [Fact]
    public async Task RemoveItemFromCartAsync_NonExistingItem_Throws()
    {
        var userId = 1;
        var productId = 2;
        var cart = new Cart { UserId = userId, CartItems = new List<CartItem>() };

        _cartRepositoryMock.Setup(r => r.GetCartByUserIdAsync(userId)).ReturnsAsync(cart);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _cartService.RemoveItemFromCartAsync(userId, productId));
    }

    [Fact]
    public async Task ClearCartByIdAsync_ClearsAndReturnsEmptyCartDto()
    {
        var userId = 1;
        var clearedCart = new Cart { UserId = userId, CartItems = new List<CartItem>() };

        _cartRepositoryMock.Setup(r => r.ClearCartByIdAsync(userId)).ReturnsAsync(clearedCart);
        _cartRepositoryMock.Setup(r => r.GetCartByUserIdAsync(userId)).ReturnsAsync(clearedCart);

        var result = await _cartService.ClearCartByIdAsync(userId);

        Assert.Equal(userId, result.UserId);
        Assert.Empty(result.CartItems);
    }

    [Fact]
    public async Task DeleteCartByIdAsync_ExistingCart_DeletesAndReturnsTrue()
    {
        var userId = 1;
        var cart = new Cart { UserId = userId };

        _cartRepositoryMock.Setup(r => r.GetCartByUserIdAsync(userId)).ReturnsAsync(cart);
        _cartRepositoryMock.Setup(r => r.DeleteCartByIdAsync(userId)).ReturnsAsync(true);

        var result = await _cartService.DeleteCartByIdAsync(userId);

        Assert.True(result);
        _cartRepositoryMock.Verify(r => r.DeleteCartByIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task DeleteCartByIdAsync_NonExistingCart_ReturnsFalse()
    {
        var userId = 1;

        _cartRepositoryMock.Setup(r => r.GetCartByUserIdAsync(userId)).ReturnsAsync((Cart)null);

        var result = await _cartService.DeleteCartByIdAsync(userId);

        Assert.False(result);
    }
}
