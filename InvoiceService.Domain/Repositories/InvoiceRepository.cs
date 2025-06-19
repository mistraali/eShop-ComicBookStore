using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InvoiceService.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace InvoiceService.Domain.Repositories;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly InvoiceDataContext _context;
    public InvoiceRepository(InvoiceDataContext context)
    {
        _context = context;
    }

    public async Task<Invoice> CreateInvoiceAsync(Invoice invoice)
    {
        await _context.Invoices.AddAsync(invoice);
        await _context.SaveChangesAsync();
        return invoice;
    }

    public async Task<InvoiceItem> CreateInvoiceItemAsync(InvoiceItem invoiceItem)
    {
        await _context.InvoiceItems.AddAsync(invoiceItem);
        await _context.SaveChangesAsync();
        return invoiceItem;
    }

    public async Task<List<Invoice>> GetAllInvoicesAsync()
    {
        return await _context.Invoices
            .Include(i => i.InvoiceItems)
            .ToListAsync();
    }

    public async Task<Invoice> GetInvoiceByInvoiceIdAsync(int invoiceId)
    {
        return await _context.Invoices
            .Include(i => i.InvoiceItems)
            .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);
    }
}
