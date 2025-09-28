using Contracts.Events;
using EventHubService.Hubs;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace EventHubService.Messaging.Consumers;

public class OrderCreatedEventConsumer(IHubContext<NotificationHub> hubContext, ILogger<OrderCreatedEventConsumer> logger)
    : IConsumer<OrderCreatedEvent>
{
    private readonly IHubContext<NotificationHub> _hubContext = hubContext;
    private readonly ILogger<OrderCreatedEventConsumer> _logger = logger;

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        await _hubContext.Clients.All.SendAsync("OrderCreated", context.Message, context.CancellationToken);
        _logger.LogInformation("Forwarded OrderCreatedEvent for Order {OrderId} to connected clients.", context.Message.OrderId);
    }
}
