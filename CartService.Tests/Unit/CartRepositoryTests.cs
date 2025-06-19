using System;
using System.Linq;
using System.Threading.Tasks;
using CartService.Domain.Models;
using CartService.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CartService.Tests.Unit;

public class CartRepositoryTests : IDisposable
{
    private readonly CartDataContext _context;
    private readonly CartRepository _repository;

    public CartRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<CartDataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new CartDataContext(options);
        _repository = new CartRepository(_context);
    }

    [Fact]
    public async Task CreateCartForUserAsync_ShouldAddCart()
    {
        var cart = new Cart { UserId = 1 };
        var result = await _repository.CreateCartForUserAsync(cart);

        Assert.NotNull(result);
        Assert.Equal(1, result.UserId);
        Assert.Single(await _context.Carts.ToListAsync());
    }

    [Fact]
    public async Task GetCartByUserIdAsync_ShouldReturnCart_WhenExists()
    {
        var cart = new Cart { UserId = 1 };
        await _context.Carts.AddAsync(cart);
        await _context.SaveChangesAsync();

        var result = await _repository.GetCartByUserIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.UserId);
    }

    [Fact]
    public async Task GetCartByUserIdAsync_ShouldReturnNull_WhenNotExists()
    {
        var result = await _repository.GetCartByUserIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllCartsAsync_ShouldReturnAllCarts()
    {
        await _context.Carts.AddRangeAsync(new Cart { UserId = 1 }, new Cart { UserId = 2 });
        await _context.SaveChangesAsync();

        var result = await _repository.GetAllCartsAsync();

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task AddItemToCartAsync_ShouldAddItem()
    {
        var item = new CartItem { CartId = 1, ProductId = 10, Quantity = 2 };

        var result = await _repository.AddItemToCartAsync(item);

        Assert.NotNull(result);
        Assert.Equal(10, result.ProductId);
        Assert.Equal(2, result.Quantity);
        Assert.Single(await _context.CartItems.ToListAsync());
    }

    [Fact]
    public async Task UpdateItemInCartAsync_ShouldUpdateExistingItem()
    {
        var cartItem = new CartItem
        {
            CartItemId = 1,
            CartId = 1,
            ProductId = 100,
            Quantity = 2
        };
        await _context.CartItems.AddAsync(cartItem);
        await _context.SaveChangesAsync();

        var updatedItem = new CartItem
        {
            CartItemId = 1,
            CartId = 1,
            ProductId = 100,
            Quantity = 5
        };

        var result = await _repository.UpdateItemInCartAsync(updatedItem);

        Assert.NotNull(result);
        Assert.Equal(5, result.Quantity);

        var itemInDb = await _context.CartItems.FindAsync(1);
        Assert.Equal(5, itemInDb.Quantity);
    }

    [Fact]
    public async Task UpdateItemInCartAsync_ShouldThrow_WhenItemDoesNotExist()
    {
        var updatedItem = new CartItem
        {
            CartItemId = 999,
            CartId = 1,
            ProductId = 100,
            Quantity = 5
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _repository.UpdateItemInCartAsync(updatedItem));
    }

    [Fact]
    public async Task RemoveItemFromCartAsync_ShouldRemoveItem_AndReturnUpdatedCart()
    {
        var userId = 1;
        var productId = 10;
        var cart = new Cart
        {
            UserId = userId,
            CartItems = new System.Collections.Generic.List<CartItem>
            {
                new CartItem { CartId = userId, ProductId = productId, Quantity = 1 },
                new CartItem { CartId = userId, ProductId = 99, Quantity = 2 }
            }
        };
        await _context.Carts.AddAsync(cart);
        await _context.SaveChangesAsync();

        var updatedCart = await _repository.RemoveItemFromCartAsync(userId, productId);

        Assert.NotNull(updatedCart);
        Assert.Equal(userId, updatedCart.UserId);
        Assert.DoesNotContain(updatedCart.CartItems, ci => ci.ProductId == productId);
        Assert.Contains(updatedCart.CartItems, ci => ci.ProductId == 99);
    }

    [Fact]
    public async Task RemoveItemFromCartAsync_ShouldThrow_WhenItemNotFound()
    {
        var userId = 1;
        var productId = 10;
        var cart = new Cart { UserId = userId };
        await _context.Carts.AddAsync(cart);
        await _context.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(() => _repository.RemoveItemFromCartAsync(userId, productId));
    }

    [Fact]
    public async Task ClearCartByIdAsync_ShouldRemoveAllItems_AndReturnEmptyCart()
    {
        var userId = 1;
        var cart = new Cart
        {
            UserId = userId,
            CartItems = new System.Collections.Generic.List<CartItem>
            {
                new CartItem { CartId = userId, ProductId = 10 },
                new CartItem { CartId = userId, ProductId = 99 }
            }
        };
        await _context.Carts.AddAsync(cart);
        await _context.SaveChangesAsync();

        var clearedCart = await _repository.ClearCartByIdAsync(userId);

        Assert.NotNull(clearedCart);
        Assert.Equal(userId, clearedCart.UserId);
        Assert.Empty(clearedCart.CartItems);
    }

    [Fact]
    public async Task DeleteCartByIdAsync_ShouldRemoveCart()
    {
        var userId = 1;
        var cart = new Cart { UserId = userId };
        await _context.Carts.AddAsync(cart);
        await _context.SaveChangesAsync();

        var result = await _repository.DeleteCartByIdAsync(userId);

        Assert.True(result);
        var cartInDb = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId);
        Assert.Null(cartInDb);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
