using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using WorkshopAPI.DTO;
using WorkshopAPI.Models;
using WorkshopsSite.Data;

namespace WorkshopAPI.Controllers;

[ApiController]
[Route("api/workshops")]
public class WorkshopsController(CatalogDB db) : ControllerBase
{
[HttpGet]
public ActionResult<List<WorkshopDTO>> Get([FromQuery] string? category)
{
    var query = db.Workshops.AsNoTracking();
    if (!string.IsNullOrWhiteSpace(category))
        query = query.Where(w => w.Category == category);

    // SQLite filters and sorts before the rows are mapped to the public JSON shape.
    var workshops = query.OrderBy(w => w.Date).ThenBy(w => w.Id)
        .AsEnumerable().Select(WorkshopDTO.From).ToList();
    return Ok(workshops);
}

[HttpGet("{id:int}")]
public ActionResult<WorkshopDTO> GetById(int id)
{
    var workshop = db.Workshops.AsNoTracking().SingleOrDefault(w => w.Id == id);
    if (workshop is null) return NotFound();
    return Ok(WorkshopDTO.From(workshop));
}

[HttpPost]
[Authorize]
[ProducesResponseType(401)]
[ProducesResponseType<WorkshopDTO>(201)]
[ProducesResponseType<ValidationProblemDetails>(400)]
public ActionResult<WorkshopDTO> Create(CreateWorkshopInput input)
{
    // [ApiController] validates the JSON before this action. SQLite supplies the ID.
    var workshop = new Workshop
    {
        Title = input.Title.Trim(),
        Category = input.Category.Trim(),
        Date = input.Date!.Value,
        Room = input.Room.Trim(),
        Description = input.Description.Trim(),
        Level = input.Level.Trim(),
        DurationMinutes = input.Duration!.Value,
        SeatsLeft = input.SeatsLeft!.Value
    };
    db.Workshops.Add(workshop);
    db.SaveChanges();

    // Location points to the JSON representation of the saved row.
    return CreatedAtAction(nameof(GetById), new { id = workshop.Id }, WorkshopDTO.From(workshop));
}

[HttpPut("{id:int}")]
[Authorize]
[ProducesResponseType(401)]
[ProducesResponseType(204)]
[ProducesResponseType(404)]
[ProducesResponseType<ValidationProblemDetails>(400)]
public IActionResult Replace([FromRoute] int id, CreateWorkshopInput input)
{
    var workshop = db.Workshops.SingleOrDefault(w => w.Id == id);
    if (workshop is null) return NotFound();

    workshop.Title = input.Title.Trim();
    workshop.Category = input.Category.Trim();
    workshop.Date = input.Date!.Value;
    workshop.Room = input.Room.Trim();
    workshop.Description = input.Description.Trim();
    workshop.Level = input.Level.Trim();
    workshop.DurationMinutes = input.Duration!.Value;
    workshop.SeatsLeft = input.SeatsLeft!.Value;

    db.SaveChanges();
    return NoContent();
}
}
