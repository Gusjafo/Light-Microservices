using Microsoft.AspNetCore.SignalR;

namespace EventHubService.Hubs;

public class NotificationHub : Hub
{
    public const string HubPath = "/hub/notifications";
    public const string BroadcastEventMethod = "BroadcastEvent";
}
