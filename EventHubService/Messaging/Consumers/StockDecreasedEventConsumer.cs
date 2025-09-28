using Contracts.Events;
using EventHubService.Hubs;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace EventHubService.Messaging.Consumers;

public class StockDecreasedEventConsumer(IHubContext<NotificationHub> hubContext, ILogger<StockDecreasedEventConsumer> logger)
    : IConsumer<StockDecreasedEvent>
{
    private readonly IHubContext<NotificationHub> _hubContext = hubContext;
    private readonly ILogger<StockDecreasedEventConsumer> _logger = logger;

    public async Task Consume(ConsumeContext<StockDecreasedEvent> context)
    {
        await _hubContext.Clients.All.SendAsync("StockDecreased", context.Message, context.CancellationToken);
        _logger.LogInformation(
            "Forwarded StockDecreasedEvent for Product {ProductId} to connected clients.",
            context.Message.ProductId);
    }
}
