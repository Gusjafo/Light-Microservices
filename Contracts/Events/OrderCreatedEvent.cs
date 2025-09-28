namespace Contracts.Events;

public record OrderCreatedEvent(
    Guid Id,
    Guid UserId,
    Guid ProductId,
    int Quantity,
    DateTime CreatedAtUtc);
