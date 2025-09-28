using Contracts.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductService.Data;

namespace ProductService.Messaging.Consumers;

public class OrderCreatedConsumer(ProductContext dbContext, ILogger<OrderCreatedConsumer> logger) : IConsumer<OrderCreatedEvent>
{
    private readonly ProductContext _dbContext = dbContext;
    private readonly ILogger<OrderCreatedConsumer> _logger = logger;

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var message = context.Message;

        var product = await _dbContext.Products
            .FirstOrDefaultAsync(p => p.Id == message.ProductId, context.CancellationToken);

        if (product is null)
        {
            _logger.LogWarning(
                "Product {ProductId} not found for order {OrderId}. Event ignored.",
                message.ProductId,
                message.Id);
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

        await _dbContext.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "Decreased stock of product {ProductId} by {Quantity} for order {OrderId}. Remaining stock: {Stock}.",
            product.Id,
            message.Quantity,
            product.Stock);
    }
}
