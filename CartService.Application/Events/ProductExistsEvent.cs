using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CartService.Application.Events;

public class ProductExistsEvent
{
    public int ProductId { get; set; }
    public bool Exists { get; set; }
}
