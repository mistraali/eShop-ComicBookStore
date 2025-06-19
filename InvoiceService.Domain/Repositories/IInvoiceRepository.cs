using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InvoiceService.Domain.Models;

namespace InvoiceService.Domain.Repositories;

public interface IInvoiceRepository
{
    Task<InvoiceItem> CreateInvoiceItemAsync(InvoiceItem invoiceItem);
    Task<Invoice> CreateInvoiceAsync(Invoice invoice);

    Task<List<Invoice>> GetAllInvoicesAsync();

    Task<Invoice> GetInvoiceByInvoiceIdAsync(int invoiceId);
}
