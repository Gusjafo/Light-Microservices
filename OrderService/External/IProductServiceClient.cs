using OrderService.Models;

namespace OrderService.External
{
    public interface IProductServiceClient
    {
        Task<ProductDto?> GetProductAsync(Guid productId, CancellationToken ct);
    }
}