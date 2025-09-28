using System.ComponentModel.DataAnnotations;

namespace OrderService.External
{
    public class ServiceEndpoints
    {
        [Required]
        public Uri User { get; set; } = null!;

        [Required]
        public Uri Product { get; set; } = null!;
    }
}
