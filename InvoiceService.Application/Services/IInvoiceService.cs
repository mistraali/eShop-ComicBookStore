using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InvoiceService.Domain.DTOs;
using InvoiceService.Domain.Models;
using InvoiceService.Domain.Events;

namespace InvoiceService.Application.Services;

public interface IInvoiceService
{
    Task<GetInvoiceDto> CreateInvoiceForUserPurchaseAsync(int userId, List<int> invoiceItems);

    //Task DeleteInvoice(int invoiceId);

    Task<Invoice> GetInvoiceByInvoiceIdAsync(int invoiceId);

    Task<List<Invoice>> GetAllInvoicesAsync();

    Task<Invoice> CreateInvoiceForCheckedOutCartAsync(CartCheckedOutEvent cartCheckedOutEvent);
}
