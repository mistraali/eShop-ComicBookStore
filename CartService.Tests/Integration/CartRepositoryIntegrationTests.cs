using System;
using System.Linq;
using System.Threading.Tasks;
using CartService.Domain.Models;
using CartService.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CartService.Tests.Integration;
public class CartRepositoryIntegrationTests : IDisposable
{
    private readonly CartDataContext _context;
    private readonly CartRepository _repository;

    public CartRepositoryIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<CartDataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new CartDataContext(options);
        _repository = new CartRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task CreateCartForUserAsync_AddsCart()
    {
        var cart = new Cart { UserId = 1 };
        var result = await _repository.CreateCartForUserAsync(cart);

        Assert.NotNull(result);
        Assert.Equal(1, result.UserId);

        var inDb = await _context.Carts.FindAsync(1);
        Assert.NotNull(inDb);
    }

    [Fact]
    public async Task GetCartByUserIdAsync_ReturnsCartWithItems()
    {
        var cart = new Cart { UserId = 1 };
        var item = new CartItem { CartId = 1, ProductId = 10, Quantity = 2 };
        await _context.Carts.AddAsync(cart);
        await _context.CartItems.AddAsync(item);
        await _context.SaveChangesAsync();

        var result = await _repository.GetCartByUserIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.UserId);
        Assert.Single(result.CartItems);
        Assert.Equal(10, result.CartItems.First().ProductId);
    }

    [Fact]
    public async Task GetAllCartsAsync_ReturnsAllCarts()
    {
        var cart1 = new Cart { UserId = 1 };
        var cart2 = new Cart { UserId = 2 };
        await _context.Carts.AddRangeAsync(cart1, cart2);
        await _context.SaveChangesAsync();

        var result = await _repository.GetAllCartsAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task AddItemToCartAsync_AddsItem()
    {
        var cart = new Cart { UserId = 1 };
        await _context.Carts.AddAsync(cart);
        await _context.SaveChangesAsync();

        var newItem = new CartItem { CartId = 1, ProductId = 20, Quantity = 3 };
        var result = await _repository.AddItemToCartAsync(newItem);

        Assert.NotNull(result);
        Assert.Equal(20, result.ProductId);

        var inDb = await _context.CartItems.FindAsync(result.CartItemId);
        Assert.NotNull(inDb);
    }

    [Fact]
    public async Task UpdateItemInCartAsync_UpdatesQuantity()
    {
        var cart = new Cart { UserId = 1 };
        var item = new CartItem { CartId = 1, ProductId = 30, Quantity = 1 };
        await _context.Carts.AddAsync(cart);
        await _context.CartItems.AddAsync(item);
        await _context.SaveChangesAsync();

        item.Quantity = 5;
        var updated = await _repository.UpdateItemInCartAsync(item);

        Assert.Equal(5, updated.Quantity);
        var inDb = await _context.CartItems.FindAsync(item.CartItemId);
        Assert.Equal(5, inDb.Quantity);
    }

    [Fact]
    public async Task UpdateItemInCartAsync_ThrowsIfItemNotFound()
    {
        var nonExistingItem = new CartItem { CartItemId = 999, CartId = 1, ProductId = 1, Quantity = 1 };

        await Assert.ThrowsAsync<InvalidOperationException>(() => _repository.UpdateItemInCartAsync(nonExistingItem));
    }

    [Fact]
    public async Task RemoveItemFromCartAsync_RemovesItem()
    {
        var cart = new Cart { UserId = 1 };
        var item1 = new CartItem { CartId = 1, ProductId = 40, Quantity = 1 };
        var item2 = new CartItem { CartId = 1, ProductId = 41, Quantity = 2 };
        await _context.Carts.AddAsync(cart);
        await _context.CartItems.AddRangeAsync(item1, item2);
        await _context.SaveChangesAsync();

        var result = await _repository.RemoveItemFromCartAsync(1, 40);

        Assert.NotNull(result);
        Assert.Single(result.CartItems);
        Assert.DoesNotContain(result.CartItems, ci => ci.ProductId == 40);
    }

    [Fact]
    public async Task ClearCartByIdAsync_RemovesAllItems()
    {
        var cart = new Cart { UserId = 1 };
        var item1 = new CartItem { CartId = 1, ProductId = 50, Quantity = 1 };
        var item2 = new CartItem { CartId = 1, ProductId = 51, Quantity = 2 };
        await _context.Carts.AddAsync(cart);
        await _context.CartItems.AddRangeAsync(item1, item2);
        await _context.SaveChangesAsync();

        var result = await _repository.ClearCartByIdAsync(1);

        Assert.NotNull(result);
        Assert.Empty(result.CartItems);
    }

    [Fact]
    public async Task DeleteCartByIdAsync_DeletesCart()
    {
        var cart = new Cart { UserId = 1 };
        await _context.Carts.AddAsync(cart);
        await _context.SaveChangesAsync();

        var result = await _repository.DeleteCartByIdAsync(1);

        Assert.True(result);
        var inDb = await _context.Carts.FindAsync(1);
        Assert.Null(inDb);
    }
}
