using System.ComponentModel.DataAnnotations;

namespace UserService.Models;

public class ProcessedEvent
{
    [Key]
    public Guid EventId { get; set; }
    public DateTime ProcessedAtUtc { get; set; }
}
