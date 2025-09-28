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
            _logger.LogWarning(
                "Product {ProductId} not found for order {OrderId}. Event ignored.",
                message.ProductId,
                message.Id);
            MarkEventAsProcessed(message.EventId);
            await _dbContext.SaveChangesAsync(context.CancellationToken);
            return;
        }

        if (product.Stock < message.Quantity)
        {
            _logger.LogWarning(
                "Insufficient stock for product {ProductId}. Available {Available}, requested {Requested}. Applying best effort decrement.",
                product.Id,
                product.Stock,
                message.Quantity);
        }

        product.Stock = Math.Max(0, product.Stock - message.Quantity);

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

    private void MarkEventAsProcessed(Guid eventId)
    {
        _dbContext.ProcessedEvents.Add(new ProcessedEvent
        {
            EventId = eventId,
            ProcessedAtUtc = DateTime.UtcNow
        });
    }
}
