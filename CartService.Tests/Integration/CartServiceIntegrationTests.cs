using CartService.Application.Infrastructure.Services;
using CartService.Application.Services;
using CartService.Domain.Models;
using CartService.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace CartService.Tests.Integration;

public class CartServiceIntegrationTests : IDisposable
{
    private readonly CartDataContext _context;
    private readonly CartRepository _repository;
    private readonly CartService.Application.Services.CartService _service;
    private readonly Mock<IProductServiceClient> _productServiceClientMock;

    public CartServiceIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<CartDataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new CartDataContext(options);
        _repository = new CartRepository(_context);

        _productServiceClientMock = new Mock<IProductServiceClient>();
        _productServiceClientMock
            .Setup(p => p.CheckIfProductExistsAsync(It.IsAny<int>()))
            .ReturnsAsync(true); // domyślnie produkt istnieje

        _service = new CartService.Application.Services.CartService(_repository, _productServiceClientMock.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task CreateCartForUserAsync_ShouldCreateCart()
    {
        var userId = 42;

        var cart = await _service.CreateCartForUserAsync(userId);

        Assert.NotNull(cart);
        Assert.Equal(userId, cart.UserId);
        Assert.Empty(cart.CartItems);
    }

    [Fact]
    public async Task GetCartByUserIdAsync_ShouldReturnCart_WhenExists()
    {
        var userId = 1;
        var cart = new Cart { UserId = userId };
        await _context.Carts.AddAsync(cart);
        await _context.SaveChangesAsync();

        var cartDto = await _service.GetCartByUserIdAsync(userId);

        Assert.NotNull(cartDto);
        Assert.Equal(userId, cartDto.UserId);
        Assert.Empty(cartDto.CartItems);
    }

    [Fact]
    public async Task GetCartByUserIdAsync_ShouldThrow_WhenCartDoesNotExist()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GetCartByUserIdAsync(999));
    }

    [Fact]
    public async Task AddItemToCartAsync_ShouldAddItem()
    {
        var userId = 5;
        await _service.CreateCartForUserAsync(userId);

        var addItemDto = new Domain.DTOs.AddItemToCartDto
        {
            CartId = userId,
            ProductId = 100,
            Quantity = 3
        };

        var addedItem = await _service.AddItemToCartAsync(addItemDto);

        Assert.NotNull(addedItem);
        Assert.Equal(userId, addedItem.CartId);
        Assert.Equal(100, addedItem.ProductId);
        Assert.Equal(3, addedItem.Quantity);
    }

    [Fact]
    public async Task RemoveItemFromCartAsync_ShouldRemoveItem()
    {
        var userId = 7;
        await _service.CreateCartForUserAsync(userId);

        var item = new CartItem
        {
            CartId = userId,
            ProductId = 555,
            Quantity = 1
        };
        await _context.CartItems.AddAsync(item);
        await _context.SaveChangesAsync();

        var cartBefore = await _service.GetCartByUserIdAsync(userId);
        Assert.Contains(555, cartBefore.CartItems);

        var updatedCart = await _service.RemoveItemFromCartAsync(userId, 555);

        Assert.DoesNotContain(555, updatedCart.CartItems);
    }

    [Fact]
    public async Task RemoveItemFromCartAsync_ShouldThrow_WhenItemNotExist()
    {
        var userId = 8;
        await _service.CreateCartForUserAsync(userId);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.RemoveItemFromCartAsync(userId, 999));
    }

    [Fact]
    public async Task ClearCartByIdAsync_ShouldRemoveAllItems()
    {
        var userId = 9;
        await _service.CreateCartForUserAsync(userId);

        var item1 = new CartItem { CartId = userId, ProductId = 1, Quantity = 1 };
        var item2 = new CartItem { CartId = userId, ProductId = 2, Quantity = 2 };
        await _context.CartItems.AddRangeAsync(item1, item2);
        await _context.SaveChangesAsync();

        var clearedCart = await _service.ClearCartByIdAsync(userId);

        Assert.Empty(clearedCart.CartItems);
    }

    [Fact]
    public async Task DeleteCartByIdAsync_ShouldDeleteCart_WhenExists()
    {
        var userId = 10;
        await _service.CreateCartForUserAsync(userId);

        var result = await _service.DeleteCartByIdAsync(userId);

        Assert.True(result);
        var cartInDb = await _repository.GetCartByUserIdAsync(userId);
        Assert.Null(cartInDb);
    }

    [Fact]
    public async Task DeleteCartByIdAsync_ShouldReturnFalse_WhenCartDoesNotExist()
    {
        var result = await _service.DeleteCartByIdAsync(999);
        Assert.False(result);
    }
}
