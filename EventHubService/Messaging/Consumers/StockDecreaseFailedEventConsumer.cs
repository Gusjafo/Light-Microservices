using Contracts.Events;
using EventHubService.Hubs;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace EventHubService.Messaging.Consumers;

public class StockDecreaseFailedEventConsumer(
    IHubContext<NotificationHub> hubContext,
    ILogger<StockDecreaseFailedEventConsumer> logger) : IConsumer<StockDecreaseFailedEvent>
{
    private readonly IHubContext<NotificationHub> _hubContext = hubContext;
    private readonly ILogger<StockDecreaseFailedEventConsumer> _logger = logger;

    public async Task Consume(ConsumeContext<StockDecreaseFailedEvent> context)
    {
        await _hubContext.Clients.All.SendAsync(
            NotificationHub.BroadcastEventMethod,
            nameof(StockDecreaseFailedEvent),
            context.Message,
            context.Message.FailedAtUtc,
            context.CancellationToken);

        _logger.LogWarning(
            "Forwarded StockDecreaseFailedEvent for Order {OrderId} to connected clients.",
            context.Message.OrderId);
    }
}
