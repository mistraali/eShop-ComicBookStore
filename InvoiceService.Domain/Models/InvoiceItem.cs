using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceService.Domain.Models;

public class InvoiceItem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int InvoiceItemId { get; set; }
    public int InvoiceId { get; set; }
    public int ProductId { get; set; }
    public string? ProductName { get; set; }
    public int Quantity { get; set; }

}
