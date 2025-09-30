namespace Contracts.Events;

public record StockDecreaseFailedEvent(
    Guid EventId,
    Guid OrderId,
    Guid ProductId,
    int RequestedQuantity,
    int AvailableStock,
    string Reason,
    DateTime FailedAtUtc);
