using System;

namespace ProductService.Models;

public class Product
{
    public Guid Id { get; set; } = Guid.NewGuid(); // Primary Key
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public int Stock { get; set; }         // Quantity available
}
