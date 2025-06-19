using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceService.Domain.DTOs;

public class GetInvoiceDto
{
    
    public int InvoiceId { get; set; }
    public int UserId { get; set; }
    public List<int> InvoiceItems { get; set; } = new(); // identyfikatory Invoice Itemów (ProductId)

}
