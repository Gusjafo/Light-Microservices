using System;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Models;

namespace ProductService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(ProductContext context) : ControllerBase
{
    private readonly ProductContext _context = context;

    // GET: api/products
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts(CancellationToken ct)
    {
        var products = await _context.Products
            .AsNoTracking()
            .ToListAsync(ct);

        return Ok(products);
    }

    // GET: api/products/1
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Product>> GetProduct(Guid id, CancellationToken ct)
    {
        var product = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (product is null) return NotFound();

        return Ok(product);
    }

    // POST: api/products
    [HttpPost]
    public async Task<ActionResult<Product>> CreateProduct(Product product, CancellationToken ct)
    {
        if (product.Id == Guid.Empty)
        {
            product.Id = Guid.NewGuid();
        }

        _context.Products.Add(product);
        await _context.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }
}
