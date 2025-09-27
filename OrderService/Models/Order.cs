namespace OrderService.Models;

public class Order
{
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
