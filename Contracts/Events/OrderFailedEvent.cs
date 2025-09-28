namespace Contracts.Events;

public record OrderFailedEvent(
    Guid EventId,
    Guid? OrderId,
    Guid UserId,
    Guid ProductId,
    string Reason,
    DateTime FailedAtUtc);
