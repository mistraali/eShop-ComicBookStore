using CartService.Application.Services;
using CartService.Domain.DTOs;
using CartService.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Validations;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CartService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private ICartService _cartService;
        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        // GET: api/<CartController>
        [HttpGet("get-carts-from-all-users")]
        public async Task<IActionResult> GetAllCarts()
        {
            var result = await _cartService.GetAllCartsAsync();
            return Ok(result);
        }

        // GET: api/<CartController>
        [HttpGet("get-users-cart-by-id")]
        public async Task<IActionResult> GetUsersCart(int userId)
        {
            var result = await _cartService.GetCartByUserIdAsync(userId);
            if (result != null)
            {
                // Return success response
                return Ok(result);
            }
            else
            {
                // Return error response
                return BadRequest(new { message = "Impossible to get user's cart. Check user Id." });
            }
        }

        // POST api/<CartController>
        [HttpPost("create-cart-for-user")]
        public async Task<IActionResult> Post(int userId)
        {
            // Call the service to create a cart for the user
            var result = await _cartService.CreateCartForUserAsync(userId);
            if (result != null)
            {
                // Return success response
                return Ok(new { message = "Cart created successfully.", userId = userId });
            }
            else
            {
                // Return error response
                return BadRequest(new { message = "Failed to create cart for the user." });
            }

        }

        // PUT api/<CartController>/5
        [HttpPut("add-item-to-cart")]
        public async Task<IActionResult> AddItemToCart([FromBody] AddItemToCartDto item)
        {
            try
            { var result = await _cartService.AddItemToCartAsync(item);
                if (result != null)
                {
                    // Return success response
                    return Ok(new { message = $"Item added to cart for user with Id: {item.CartId}." });
                }
                else
                {
                    // Return error response
                    return BadRequest(new { message = "Cannot add item to cart. Check your input" });
                }
            }
            catch (ProductNotFoundException ex)
            {
                // Return error response if product not found
                return NotFound(new { message = $"Product with Id: {item.ProductId} does not exist." });
            }
        }

        // DELETE api/<CartController>/5
        [HttpDelete("remove-item-from-cart")]
        public async Task<IActionResult> RemoveItemFromCart(int userId, int productId)
        {
            try
            {
                await _cartService.RemoveItemFromCartAsync(userId, productId);
                // Return success response
                return Ok(new { message = $"Item with Id: {productId} removed from cart for user with Id: {userId}." });
            }
            catch (InvalidOperationException ex)
            {
                // Return error response
                return BadRequest(new { message = "Item cannot be removed from cart." });
            }
        }

        // PUT api/<CartController>/5
        [HttpPut("clear-cart-by-user-id")]
        public async Task<IActionResult> ClearCartById(int userId)
        {
            var result = await _cartService.ClearCartByIdAsync(userId);
            if (result != null)
            {
                // Return success response
                return Ok(new { message = $"Cart for user with Id: {userId} was cleared." });
            }
            else
            {
                // Return error response
                return BadRequest(new { message = "Cannot clear the cart." });
            }
        }

        // DELETE api/<CartController>/5
        [HttpDelete("delete-cart-by-user-id")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteCartById(int userId)
        {
            var result = await _cartService.DeleteCartByIdAsync(userId);
            if (result == true)
            {
                // Return success response
                return Ok(new { message = $"Cart for user with Id: {userId} was deleted." });
            }
            else
            {
                // Return error response
                return BadRequest(new { message = "Cannot delete the cart." });
            }
        }

        // DELETE api/<CartController>/5
        [HttpDelete("cart-checkout-by-user-id")]
        public async Task<IActionResult> CartCheckoutById(int userId)
        {
            var result = await _cartService.CartCheckoutByIdAsync(userId);
            if (result == true)
            {
                // Return success response
                return Ok(new { message = $"Cart for user with Id: {userId} was checked-out." });
            }
            else
            {
                // Return error response
                return BadRequest(new { message = "Cannot checkout the cart." });
            }
        }
    }
}
