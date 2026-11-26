using System.ComponentModel.DataAnnotations;

namespace WorkshopAPI.DTO;

public class AccountCredentials
{
    [Required, RegularExpression(@"[a-zA-Z0-9_.-]{3,40}", ErrorMessage = "Use 3–40 letters, digits, dots, hyphens or underscores for the username.")]
    public string UserName { get; set; } = "";

    [Required, StringLength(128, MinimumLength = 12)]
    public string Password { get; set; } = "";
}

public record LoginResult(string UserName, string ApiKey);
