namespace Contracts.Events;

public record StockDecreasedEvent(
    Guid EventId,
    Guid OrderId,
    Guid ProductId,
    int Quantity,
    int RemainingStock,
    DateTime ProcessedAtUtc);
