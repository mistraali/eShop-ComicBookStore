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
        [HttpPost]
        public void Post([FromBody] string value)
        {
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
