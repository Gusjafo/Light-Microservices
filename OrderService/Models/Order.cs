namespace OrderService.Models;

public class Order
{
    public int Id { get; set; }              // Primary key
    public int UserId { get; set; }          // FK reference to User Service
    public int ProductId { get; set; }       // FK reference to Product Service
    public int Quantity { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
