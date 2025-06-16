using CartService.Application.Services;
using CartService.Domain.DTOs;
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

        //// PUT api/<CartController>/5
        //[HttpPut("{id}")]
        //public void AddItemToCart(int userId, [FromBody] AddItemToCartDto item)
        //{
        //    var result = await _cartService.AddItemToCartAsync(userId, item);
        //    if (result != null)
        //    {
        //        // Return success response
        //        return Ok(new { message = $"Item added to cart for user with Id: {userId}.");
        //    }
        //    else
        //    {
        //        // Return error response
        //        return BadRequest(new { message = "Cannot add item to cart." });
        //    }
        //}

        // DELETE api/<CartController>/5
        [HttpDelete("{id}")]
        public void DeleteUsersCart(int userId)
        {
        }
    }
}
