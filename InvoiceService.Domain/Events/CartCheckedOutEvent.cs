using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InvoiceService.Domain.DTOs;

namespace InvoiceService.Domain.Events;

public class CartCheckedOutEvent
{
    public int UserId { get; set; }
    public List<GetCartItemDto> CartItems { get; set; } = new List<GetCartItemDto>();
}
