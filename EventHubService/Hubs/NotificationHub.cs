using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace EventHubService.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    public const string HubPath = "/hub/notifications";
    public const string BroadcastEventMethod = "BroadcastEvent";
}
