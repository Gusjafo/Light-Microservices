using Contracts.Events;
using EventHubService.Hubs;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace EventHubService.Messaging.Consumers;

public class OrderFailedEventConsumer(IHubContext<NotificationHub> hubContext, ILogger<OrderFailedEventConsumer> logger)
    : IConsumer<OrderFailedEvent>
{
    private readonly IHubContext<NotificationHub> _hubContext = hubContext;
    private readonly ILogger<OrderFailedEventConsumer> _logger = logger;

    public async Task Consume(ConsumeContext<OrderFailedEvent> context)
    {
        await _hubContext.Clients.All.SendAsync(
            NotificationHub.BroadcastEventMethod,
            nameof(OrderFailedEvent),
            context.Message,
            context.Message.FailedAtUtc,
            context.CancellationToken);
        _logger.LogInformation(
            "Forwarded OrderFailedEvent for Order {OrderId} to connected clients.",
            context.Message.OrderId);
    }
}
