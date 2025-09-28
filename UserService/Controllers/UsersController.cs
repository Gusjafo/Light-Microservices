using System;
using System.Collections.Generic;
using System.Threading;
using Contracts.Events;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserService.Data;
using UserService.Models;

namespace UserService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(UserContext context, IPublishEndpoint publishEndpoint, ILogger<UsersController> logger) : ControllerBase
{
    private readonly UserContext _context = context;
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;
    private readonly ILogger<UsersController> _logger = logger;

    // GET: api/users
    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers(CancellationToken ct)
    {
        var users = await _context.Users
            .AsNoTracking()
            .ToListAsync(ct);

        return Ok(users);
    }

    // GET: api/users/1
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<User>> GetUser(Guid id, CancellationToken ct)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, ct);

        if (user is null) return NotFound();

        return Ok(user);
    }

    // POST: api/users
    [HttpPost]
    public async Task<ActionResult<User>> CreateUser(User user, CancellationToken ct)
    {
        if (user.Id == Guid.Empty)
        {
            user.Id = Guid.NewGuid();
        }

        _context.Users.Add(user);
        await _context.SaveChangesAsync(ct);

        await _publishEndpoint.Publish(new UserCreatedEvent(
            Guid.NewGuid(),
            user.Id,
            user.Name,
            user.Email,
            DateTime.UtcNow), ct);

        _logger.LogInformation("User {UserId} created and event published.", user.Id);

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    // PUT: api/users/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateUser(Guid id, User updatedUser, CancellationToken ct)
    {
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (existingUser is null) return NotFound();

        existingUser.Name = updatedUser.Name;
        existingUser.Email = updatedUser.Email;

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("User {UserId} updated.", id);

        return NoContent();
    }

    // DELETE: api/users/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteUser(Guid id, CancellationToken ct)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (user is null) return NotFound();

        _context.Users.Remove(user);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("User {UserId} deleted.", id);

        return NoContent();
    }
}
