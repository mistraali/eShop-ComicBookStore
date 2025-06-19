using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InvoiceService.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace InvoiceService.Domain.Repositories;

public class InvoiceDataContext : DbContext
{
    public InvoiceDataContext(DbContextOptions<InvoiceDataContext> options) : base(options) { }

    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<InvoiceItem> InvoiceItems { get; set; }
}
