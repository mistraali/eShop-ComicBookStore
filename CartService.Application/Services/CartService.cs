﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CartService.Domain.DTOs;
using CartService.Domain.Models;
using CartService.Domain.Repositories;
using CartService.Application.Infrastructure.Services;
using CartService.Domain.Exceptions;
using CartService.Application.Kafka;
using CartService.Domain.Events;

namespace CartService.Application.Services;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductServiceClient _productServiceClient;
    private readonly CartToInvoiceKafkaProducer _kafkaProducer;
    public CartService(ICartRepository cartRepository, IProductServiceClient productServiceClient, CartToInvoiceKafkaProducer kafkaProducer)
    {
        _cartRepository = cartRepository;
        _productServiceClient = productServiceClient;
        _kafkaProducer = kafkaProducer;
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
       //Check if product exists in ProductService
            var productExists = await _productServiceClient.CheckIfProductExistsAsync(item.ProductId);
            if (!productExists)
            {
                throw new ProductNotFoundException(item.ProductId);
            }

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

    public async Task<GetCartDto> RemoveItemFromCartAsync(int userId, int productId)
    {
        var cart = await _cartRepository.GetCartByUserIdAsync(userId);
        if (cart == null)
        {
            throw new InvalidOperationException($"Cart for user with id: {userId} does not exist.");
        }
        var itemToRemove = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
        if (itemToRemove == null)
        {
            throw new InvalidOperationException($"Item with product id: {productId} does not exist in the cart for user with id: {userId}.");
        }

        await _cartRepository.RemoveItemFromCartAsync(userId, productId);

        var updatedCart = await _cartRepository.GetCartByUserIdAsync(userId);

        var cartDto = new GetCartDto
        {
            UserId = updatedCart.UserId,
            CartItems = updatedCart.CartItems.Select(c => c.ProductId).ToList()
        };

        return cartDto;
    }

    public async Task<GetCartDto> ClearCartByIdAsync(int userId)
    {
        await _cartRepository.ClearCartByIdAsync(userId);

        var cart = await _cartRepository.GetCartByUserIdAsync(userId);

        var cartDto = new GetCartDto
        {
            UserId = cart.UserId,
            CartItems = cart.CartItems.Select(c => c.ProductId).ToList()
        };
        return cartDto;
    }

    public async Task<bool> DeleteCartByIdAsync(int userId)
    {
        var cart = await _cartRepository.GetCartByUserIdAsync(userId);
        if (cart == null)
        {
            return false; // cart does not exist
        }
        
        await _cartRepository.DeleteCartByIdAsync(cart.UserId);
        return true; 
    }

    public async Task<bool> CartCheckoutByIdAsync(int userId)
    {

        var cart = await _cartRepository.GetCartByUserIdAsync(userId);
        if (cart == null || !cart.CartItems.Any())
        {
            return false; // cart does not exist or is empty
        }

        // Send cart to Invoice Service via Kafka
        await _kafkaProducer.PublishCartChekedOutAsync(new CartCheckedOutEvent
        {
            UserId = userId,
            CartItems = cart.CartItems.Select(ci => new GetCartItemDto
            {
                ProductId = ci.ProductId,
                Quantity = ci.Quantity,
                CartItemId = ci.CartItemId,
                CartId = ci.CartId  
            }).ToList()
        });

        // Clear the cart after checkout
        await _cartRepository.ClearCartByIdAsync(userId);

        return true; // checkout successful
    }
}
