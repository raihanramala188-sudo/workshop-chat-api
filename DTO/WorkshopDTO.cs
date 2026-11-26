using WorkshopAPI.Models;

namespace WorkshopAPI.DTO;

public record WorkshopDTO(int Id, string Title, string Description, int Duration,
    DateOnly Date, string Level, string Category, string Room, int SeatsLeft)
{
    // Earlier A25 clients read this spelling. New code uses the description property.
    public string Descrition => Description;

    public static WorkshopDTO From(Workshop w) =>
        new(w.Id, w.Title, w.Description, w.DurationMinutes, w.Date, w.Level, w.Category, w.Room, w.SeatsLeft);
}
