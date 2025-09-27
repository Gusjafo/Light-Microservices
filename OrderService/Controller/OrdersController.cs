using Microsoft.AspNetCore.Mvc;
using OrderService.Models;
using OrderService.Services;

namespace OrderService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController(IOrderCreationService service) : ControllerBase
{
    private readonly IOrderCreationService _service = service;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest req, CancellationToken ct)
    {
        var (ok, error, order) = await _service.CreateAsync(req.UserId, req.ProductId, req.Quantity, ct);
        if (!ok) return BadRequest(new { error });
        return CreatedAtAction(nameof(GetById), new { id = order!.Id }, order);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var order = await _service.GetAsync(id, ct);
        return order is null ? NotFound() : Ok(order);
    }
}
