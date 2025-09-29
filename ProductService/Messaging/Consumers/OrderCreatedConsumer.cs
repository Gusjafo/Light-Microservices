using Contracts.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductService.Data;
using ProductService.Models;

namespace ProductService.Messaging.Consumers;

public class OrderCreatedConsumer(
    ProductContext dbContext,
    ILogger<OrderCreatedConsumer> logger,
    IPublishEndpoint publishEndpoint) : IConsumer<OrderCreatedEvent>
{
    private readonly ProductContext _dbContext = dbContext;
    private readonly ILogger<OrderCreatedConsumer> _logger = logger;
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var message = context.Message;

        var alreadyProcessed = await _dbContext.ProcessedEvents
            .AnyAsync(e => e.EventId == message.EventId, context.CancellationToken);
        if (alreadyProcessed)
        {
            _logger.LogInformation(
                "Skipping already processed event {EventId} for order {OrderId}.",
                message.EventId,
                message.Id);
            return;
        }

        var product = await _dbContext.Products
            .FirstOrDefaultAsync(p => p.Id == message.ProductId, context.CancellationToken);

        if (product is null)
        {
            const string reason = "Product not found.";
            _logger.LogWarning(
                "Product {ProductId} not found for order {OrderId}. Event ignored.",
                message.ProductId,
                message.Id);
            MarkEventAsProcessed(message.EventId);
            await _dbContext.SaveChangesAsync(context.CancellationToken);
            await PublishFailureAsync(message, reason, 0, context.CancellationToken);
            return;
        }

        if (product.Stock < message.Quantity)
        {
            var reason =
                $"Insufficient stock. Available: {product.Stock}. Requested: {message.Quantity}.";
            _logger.LogWarning(
                "Insufficient stock for product {ProductId}. Available {Available}, requested {Requested}.",
                product.Id,
                product.Stock,
                message.Quantity);
            MarkEventAsProcessed(message.EventId);
            await _dbContext.SaveChangesAsync(context.CancellationToken);
            await PublishFailureAsync(message, reason, product.Stock, context.CancellationToken);
            return;
        }

        product.Stock -= message.Quantity;

        MarkEventAsProcessed(message.EventId);
        await _dbContext.SaveChangesAsync(context.CancellationToken);

        await _publishEndpoint.Publish(new StockDecreasedEvent(
            Guid.NewGuid(),
            message.Id,
            product.Id,
            message.Quantity,
            product.Stock,
            DateTime.UtcNow),
            context.CancellationToken);

        _logger.LogInformation(
            "Decreased stock of product {ProductId} by {Quantity} for order {OrderId}. Remaining stock: {Stock}.",
            product.Id,
            message.Quantity,
            product.Stock);
    }

    private async Task PublishFailureAsync(
        OrderCreatedEvent order,
        string reason,
        int availableStock,
        CancellationToken cancellationToken)
    {
        await _publishEndpoint.Publish(new StockDecreaseFailedEvent(
            Guid.NewGuid(),
            order.Id,
            order.ProductId,
            order.Quantity,
            availableStock,
            reason,
            DateTime.UtcNow),
            cancellationToken);

        _logger.LogWarning(
            "Published StockDecreaseFailedEvent for order {OrderId}. Reason: {Reason}",
            order.Id,
            reason);
    }

    private void MarkEventAsProcessed(Guid eventId)
    {
        _dbContext.ProcessedEvents.Add(new ProcessedEvent
        {
            EventId = eventId,
            ProcessedAtUtc = DateTime.UtcNow
        });
    }
}
