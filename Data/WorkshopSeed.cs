using WorkshopAPI.Models;

namespace WorkshopsSite.Data
{
    internal static class WorkshopSeed
    {
        internal static readonly Workshop[] Rows =
    [
        new()
        {
            Id = 1, Title = "Typography for the Web", Category = "Design",
            Date = new DateOnly(2027, 3, 2), Room = "Studio B",
            Description = "Make a campus announcement easier to read on a phone.",
            Level = "Beginner", DurationMinutes = 60, SeatsLeft = 8
        },
        new()
        {
            Id = 2, Title = "Stories from Campus Sensors", Category = "Data",
            Date = new DateOnly(2027, 3, 4), Room = "Room C12",
            Description = "Use readings from campus sensors to explain how a room changes throughout the day.",
            Level = "Beginner", DurationMinutes = 90, SeatsLeft = 0
        },
        new()
        {
            Id = 3, Title = "Accessible Sign-up Forms", Category = "Web",
            Date = new DateOnly(2027, 3, 9), Room = "Lab A",
            Description = "Explore how labels and keyboard navigation help students complete a sign-up form.",
            Level = "Beginner", DurationMinutes = 90, SeatsLeft = 12
        },
        new()
        {
            Id = 4, Title = "Color Systems for Campus Maps", Category = "Design",
            Date = new DateOnly(2027, 3, 12), Room = "Studio B",
            Description = "Choose a consistent set of colors for a readable campus map.",
            Level = "Intermediate", DurationMinutes = 120, SeatsLeft = 0
        },
        new()
        {
            Id = 5, Title = "Cleaning a Small Dataset", Category = "Data",
            Date = new DateOnly(2027, 3, 16), Room = "Room C12",
            Description = "Find missing values and prepare a small dataset for analysis.",
            Level = "Intermediate", DurationMinutes = 120, SeatsLeft = 6
        },
        new()
        {
            Id = 6, Title = "Keyboard Friendly Menus", Category = "Web",
            Date = new DateOnly(2027, 3, 19), Room = "Lab A",
            Description = "Test a simple navigation menu without using a mouse.",
            Level = "Intermediate", DurationMinutes = 60, SeatsLeft = 4
        }
    ];
    }
}
