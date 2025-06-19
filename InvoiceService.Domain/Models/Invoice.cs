using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceService.Domain.Models;

public class Invoice
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int InvoiceId { get; set; }
    [Required]
    public int UserId { get; set; }
    public DateTime InvoiceDate { get; set; } = DateTime.Now;
    public List<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
}
