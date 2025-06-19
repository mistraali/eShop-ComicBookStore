using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceService.Application.Infrastructure.Services;

public interface IProductServiceClient
{
    Task<string> GetProductNameByIdAsync(int productId);
}
