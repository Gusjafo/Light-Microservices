using System.Collections.Generic;
using System.Linq;
using Contracts.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.External;
using OrderService.Models;

namespace OrderService.Services
{
    public class OrderCreationService(
        OrderContext db,
        IUserServiceClient users,
        IProductServiceClient products,
        IPublishEndpoint publishEndpoint) : IOrderCreationService
    {
        private readonly OrderContext _db = db;
        private readonly IUserServiceClient _users = users;
        private readonly IProductServiceClient _products = products;
        private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;

        public async Task<(bool Ok, string? Error, Order? Order)> CreateAsync(Guid userId, Guid productId, int quantity, CancellationToken ct)
        {
            if (quantity <= 0) return (false, "Quantity must be > 0.", null);

            // 1) Check user
            var userExists = await _users.UserExistsAsync(userId, ct);
            if (!userExists)
            {
                await PublishOrderFailedAsync(null, userId, productId, $"User {userId} not found.", ct);
                return (false, $"User {userId} not found.", null);
            }

            // 2) Check product + stock
            var product = await _products.GetProductAsync(productId, ct);
            if (product is null)
            {
                await PublishOrderFailedAsync(null, userId, productId, $"Product {productId} not found.", ct);
                return (false, $"Product {productId} not found.", null);
            }

            if (product.Stock < quantity)
            {
                await PublishOrderFailedAsync(null, userId, productId, $"Insufficient stock. Available: {product.Stock}.", ct);
                return (false, $"Insufficient stock. Available: {product.Stock}.", null);
            }

            // 3) Persist order
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
