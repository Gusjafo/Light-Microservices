namespace Contracts.Events;

public record UserCreatedEvent(
    Guid EventId,
    Guid UserId,
    string Name,
    string Email,
    DateTime CreatedAtUtc);
