namespace CartService.Application.Infrastructure.Services;

public interface IProductServiceClient
{
    Task<bool> CheckIfProductExistsAsync(int productId);
}
