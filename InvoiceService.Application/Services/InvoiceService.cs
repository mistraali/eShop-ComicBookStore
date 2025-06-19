using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InvoiceService.Domain.Events;
using InvoiceService.Domain.DTOs;
using InvoiceService.Domain.Models;
using InvoiceService.Domain.Repositories;

namespace InvoiceService.Application.Services;

public class InvoiceService : IInvoiceService
{
    private readonly IInvoiceRepository _invoiceRepository;
    public InvoiceService(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<GetInvoiceDto> CreateInvoiceForUserPurchaseAsync(int userId, List<int> invoiceItems)
    {
        var invoice = new Invoice { UserId = userId };
        var result = await _invoiceRepository.CreateInvoiceAsync(invoice);


        foreach (var item in invoiceItems)
        {
            //invoice.InvoiceItems.Add(new InvoiceItem { ProductId = item });
            var invoiceItem = new InvoiceItem { ProductId = item, InvoiceId = result.InvoiceId };
            var itemResult = await _invoiceRepository.CreateInvoiceItemAsync(invoiceItem);
        }
        
        var dto = new GetInvoiceDto
        {
            InvoiceId = result.InvoiceId,
            UserId = result.UserId,
            InvoiceItems = result.InvoiceItems.Select(i => i.ProductId).ToList(),
        };
        return dto;
    }

    public async Task<List<Invoice>> GetAllInvoicesAsync()
    {
        return await _invoiceRepository.GetAllInvoicesAsync();
    }

    public async Task<Invoice> GetInvoiceByInvoiceIdAsync(int invoiceId)
    {
        var invoice = await _invoiceRepository.GetInvoiceByInvoiceIdAsync(invoiceId);
        if (invoice == null)
        {
            throw new InvalidOperationException($"Invoice with id: {invoiceId} does not exist.");
        }

        return invoice;
    }

    public async Task<Invoice> CreateInvoiceForCheckedOutCartAsync(CartCheckedOutEvent cartCheckedOutEvent)
    {
        var invoice = new Invoice { UserId = cartCheckedOutEvent.UserId };
        var result = await _invoiceRepository.CreateInvoiceAsync(invoice);


        foreach (var item in cartCheckedOutEvent.CartItems)
        {
            var invoiceItem = new InvoiceItem { 
                ProductId = item.ProductId, 
                InvoiceId = result.InvoiceId,
                Quantity  = item.Quantity
            };
            var itemResult = await _invoiceRepository.CreateInvoiceItemAsync(invoiceItem);
        }
        return result;
    }
}
