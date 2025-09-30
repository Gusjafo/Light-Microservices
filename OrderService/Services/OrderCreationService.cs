using System.Collections.Generic;
using System.Linq;
using Contracts.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Models;

namespace OrderService.Services
{
    public class OrderCreationService(
        OrderContext db,
        IPublishEndpoint publishEndpoint) : IOrderCreationService
    {
        private readonly OrderContext _db = db;
        private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;

        public async Task<(bool Ok, string? Error, Order? Order)> CreateAsync(Guid userId, Guid productId, int quantity, CancellationToken ct)
        {
            if (quantity <= 0)
            {
                const string error = "Quantity must be > 0.";
                await PublishOrderFailedAsync(null, userId, productId, error, ct);
                return (false, error, null);
            }

            // Persist order and emit integration event for downstream processing
            var order = new Order { UserId = userId, ProductId = productId, Quantity = quantity };
            _db.Orders.Add(order);
            await _db.SaveChangesAsync(ct);

            await _publishEndpoint.Publish(new OrderCreatedEvent(
                Guid.NewGuid(),
                order.Id,
                order.UserId,
                order.ProductId,
                order.Quantity,
                order.CreatedAtUtc), ct);

            return (true, null, order);
        }

        private Task PublishOrderFailedAsync(Guid? orderId, Guid userId, Guid productId, string reason, CancellationToken ct) =>
            _publishEndpoint.Publish(new OrderFailedEvent(
                Guid.NewGuid(),
                orderId,
                userId,
                productId,
                reason,
                DateTime.UtcNow), ct);

        public async Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken ct) =>
            await _db.Orders
                .AsNoTracking()
                .OrderByDescending(o => o.CreatedAtUtc)
                .ToListAsync(ct);

        public Task<Order?> GetAsync(Guid id, CancellationToken ct) =>
            _db.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id, ct);
    }
}
