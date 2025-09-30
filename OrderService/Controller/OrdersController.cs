using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.Models;
using OrderService.Services;

namespace OrderService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController(IOrderCreationService service) : ControllerBase
{
    private readonly IOrderCreationService _service = service;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Order>>> GetAll(CancellationToken ct)
    {
        var orders = await _service.GetAllAsync(ct);
        return Ok(orders);
    }

    [HttpPost]
    [Authorize(Roles = "User,Admin")]
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
