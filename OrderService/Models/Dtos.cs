using System;

namespace OrderService.Models
{
    // Request from client to create an order
    public record CreateOrderRequest(Guid UserId, Guid ProductId, int Quantity);

    // What we need from Product Service
    public record ProductDto(Guid Id, string Name, decimal Price, int Stock);
}
