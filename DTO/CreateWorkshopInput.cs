using System.ComponentModel.DataAnnotations;

namespace WorkshopAPI.DTO;

public class CreateWorkshopInput
{
    [Required, StringLength(80)]
    public string Title { get; set; } = "";
    [Required, StringLength(60)]
    public string Category { get; set; } = "";
    [Required]
    public DateOnly? Date { get; set; }
    [Required, StringLength(60)]
    public string Room { get; set; } = "";
    [Required, StringLength(240)]
    public string Description { get; set; } = "";
    [Required, StringLength(60)]
    public string Level { get; set; } = "";
    [Required, Range(1, 240)]
    public int? Duration { get; set; }
    // A missing seat count is invalid; zero is a valid, fully booked workshop.
    [Required, Range(0, int.MaxValue)]
    public int? SeatsLeft { get; set; }
}
