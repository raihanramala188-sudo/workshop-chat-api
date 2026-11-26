namespace WorkshopAPI.Models;

public class CampusUser
{
    public int Id { get; set; }
    public required string UserName { get; set; }
    public required string PasswordHash { get; set; }
    public string? ApiKeyHash { get; set; }
}
