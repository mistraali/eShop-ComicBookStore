using InvoiceService.Domain.Events;
using InvoiceService.Application.Services;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace InvoiceService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceController : ControllerBase
    {

        private IInvoiceService _invoiceService;
        public InvoiceController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }


        // GET: api/<InvoiceController>
        [HttpGet("get-all-invoices")]
        public async Task<IActionResult> GetAllInvoices()
        {
            var result = await _invoiceService.GetAllInvoicesAsync();
            if (result != null && result.Any())
            {
                // Return success response
                return Ok(result);
            }
            else
            {
                // Return error response
                return NotFound(new { message = "No invoices found." });
            }
        }

        // GET api/<InvoiceController>/5
        [HttpGet("get-invoice-by-InvoiceId")]
        public async Task<IActionResult> GetInvoiceByInvoiceIdAsync(int invoiceId)
        {
            var result = await _invoiceService.GetInvoiceByInvoiceIdAsync(invoiceId);
            if (result != null)
            {
                // Return success response
                return Ok(result);
            }
            else
            {
                // Return error response
                return NotFound(new { message = "Invoice not found." });
            }
        }

        // POST api/<InvoiceController>
        [HttpPost("create-invoice-for-user-purchase-manually")]
        public async Task<IActionResult> CreateInvoiceForUserPurchase(int userId, List<int> invoiceItems)
        {
            var result = await _invoiceService.CreateInvoiceForUserPurchaseAsync(userId, invoiceItems);
            if (result != null)
            {
                // Return success response
                return Ok(new { message = "Invoice created successfully.", invoiceId = result.InvoiceId });
            }
            else
            {
                // Return error response
                return BadRequest(new { message = "Impossible to create invoice. Check user Id and items." });
            }               
        }

        // POST api/<InvoiceController>
        [HttpPost("create-invoice-for-user-from-checkedout-cart")]
        public async Task<IActionResult> CreateInvoiceForCheckedOutCart([FromBody] CartCheckedOutEvent cartCheckedOutEvent)
        {
            var result = await _invoiceService.CreateInvoiceForCheckedOutCartAsync(cartCheckedOutEvent);
            if (result != null)
            {
                // Return success response
                return Ok(new { message = "Invoice created successfully.", invoiceId = result.InvoiceId });
            }
            else
            {
                // Return error response
                return BadRequest(new { message = "Impossible to create invoice. Check payload." });
            }
        }
    }
}
