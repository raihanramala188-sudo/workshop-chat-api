using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkshopAPI.Models;
using WorkshopsSite.Data;

namespace WorkshopAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController(CatalogDB db) : ControllerBase
{
    public sealed record CreateMessageRequest(string Text, int? WorkshopId);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ChatMessage>>> GetMessages(
        [FromQuery] int? workshopId = null,
        [FromQuery] int limit = 50)
    {
        var safeLimit = Math.Clamp(limit, 1, 200);
        var query = db.Messages.AsNoTracking().AsQueryable();

        if (workshopId.HasValue)
        {
            query = query.Where(m => m.WorkshopId == workshopId.Value);
        }

        var items = await query
            .OrderByDescending(m => m.Id)
            .Take(safeLimit)
            .OrderBy(m => m.Id)
            .ToListAsync();

        return Ok(items);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ChatMessage>> PostMessage([FromBody] CreateMessageRequest request)
    {
        var trimmed = (request.Text ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return BadRequest(new { title = "Message text is required." });
        }

        if (trimmed.Length > 1000)
        {
            return BadRequest(new { title = "Message text must be 1000 characters or fewer." });
        }

        var sender = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(sender))
        {
            sender = User.FindFirst("name")?.Value ?? User.FindFirst("sub")?.Value ?? "Signed-in user";
        }

        var message = new ChatMessage
        {
            WorkshopId = request.WorkshopId,
            Sender = sender,
            Text = trimmed,
            CreatedAtUtc = DateTime.UtcNow
        };

        db.Messages.Add(message);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMessages), new { id = message.Id }, message);
    }
}
