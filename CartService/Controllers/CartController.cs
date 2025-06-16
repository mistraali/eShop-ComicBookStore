using CartService.Application.Services;
using Microsoft.AspNetCore.Mvc;

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
            return Ok("get all carts");
        }

        // GET: api/<CartController>
        [HttpGet("get-users-cart-by-id")]
        public async Task<IActionResult> GetUsersCart(int userId)
        {
            return Ok("get users cart");
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
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<CartController>/5
        [HttpDelete("{id}")]
        public void DeleteUsersCart(int userId)
        {
        }
    }
}
