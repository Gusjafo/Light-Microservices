using System.ComponentModel.DataAnnotations;

namespace OrderService.Models;

public class ProcessedEvent
{
    [Key]
    public Guid EventId { get; set; }
    public DateTime ProcessedAtUtc { get; set; }
}
