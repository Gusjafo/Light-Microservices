using OrderService.Models;

namespace OrderService.Services
{
    public interface IOrderCreationService
    {
        Task<(bool Ok, string? Error, Order? Order)> CreateAsync(Guid userId, Guid productId, int quantity, CancellationToken ct);
        Task<Order?> GetAsync(Guid id, CancellationToken ct);
    }
}