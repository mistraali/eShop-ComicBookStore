using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductService.Application.Events;

public class ProductExistsEvent
{
    public int ProductId { get; set; }
    public bool Exists { get; set; }
}
