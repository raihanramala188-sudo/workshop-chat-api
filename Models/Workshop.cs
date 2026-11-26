namespace WorkshopAPI.Models
{
    public class Workshop
    {
        public int Id { get; set; }

        public required string Title { get; set; }

        public required string Category { get; set; }

        public DateOnly Date { get; set; }

        public required string Room { get; set; }

        public required string Description { get; set; }

        public required string Level { get; set; }

        public int DurationMinutes { get; set; }

        public int SeatsLeft { get; set; }
    }
}
