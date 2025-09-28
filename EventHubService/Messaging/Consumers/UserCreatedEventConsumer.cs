using Contracts.Events;
using EventHubService.Hubs;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace EventHubService.Messaging.Consumers;

public class UserCreatedEventConsumer(IHubContext<NotificationHub> hubContext, ILogger<UserCreatedEventConsumer> logger)
    : IConsumer<UserCreatedEvent>
{
    private readonly IHubContext<NotificationHub> _hubContext = hubContext;
    private readonly ILogger<UserCreatedEventConsumer> _logger = logger;

    public async Task Consume(ConsumeContext<UserCreatedEvent> context)
    {
        await _hubContext.Clients.All.SendAsync(
            NotificationHub.BroadcastEventMethod,
            nameof(UserCreatedEvent),
            context.Message,
            context.Message.CreatedAtUtc,
            context.CancellationToken);
        _logger.LogInformation("Forwarded UserCreatedEvent for User {UserId} to connected clients.", context.Message.UserId);
    }
}
