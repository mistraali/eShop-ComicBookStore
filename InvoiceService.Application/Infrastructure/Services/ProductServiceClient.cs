using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using InvoiceService.Domain.Models;

namespace InvoiceService.Application.Infrastructure.Services;

public class ProductServiceClient : IProductServiceClient
{
    private readonly HttpClient _httpClient;

    public ProductServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GetProductNameByIdAsync(int productId)
    {
        var response = await _httpClient.GetAsync($"/api/Books/{productId}");

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return "Book does not exist.";

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var product = JsonSerializer.Deserialize<Book>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return product?.Name ?? "Unknown";
    }
}
