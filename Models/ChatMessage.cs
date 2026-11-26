namespace WorkshopAPI.Models;

public class ChatMessage
{
    public int Id { get; set; }
    public int? WorkshopId { get; set; }
    public string Sender { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
