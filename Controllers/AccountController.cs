using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WorkshopAPI.DTO;
using WorkshopAPI.Models;
using WorkshopAPI.Security;
using WorkshopsSite.Data;

namespace WorkshopAPI.Controllers;

[ApiController]
[Route("api/auth")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class AccountController(CatalogDB db, IPasswordHasher<CampusUser> passwords) : ControllerBase
{
    [HttpPost("signup")]
    [ProducesResponseType(201)]
    [ProducesResponseType<ProblemDetails>(409)]
    public IActionResult Signup(AccountCredentials input)
    {
        var user = new CampusUser { UserName = input.UserName, PasswordHash = "" };
        user.PasswordHash = passwords.HashPassword(user, input.Password);
        db.Users.Add(user);
        try
        {
            db.SaveChanges();
        }
        catch (DbUpdateException e) when (e.InnerException is SqliteException { SqliteExtendedErrorCode: 2067 })
        {
            // The unique username index also handles simultaneous signups.
            return Problem(statusCode: 409, title: "That username is already in use.");
        }
        return StatusCode(201);
    }

    [HttpPost("login")]
    [ProducesResponseType<LoginResult>(200)]
    [ProducesResponseType<ProblemDetails>(401)]
    public ActionResult<LoginResult> Login(AccountCredentials input)
    {
        var user = db.Users.SingleOrDefault(u => u.UserName == input.UserName);
        if (user is null)
            return Problem(statusCode: 401, title: "Incorrect username or password.");
        var verified = passwords.VerifyHashedPassword(user, user.PasswordHash, input.Password);
        if (verified == PasswordVerificationResult.Failed)
            return Problem(statusCode: 401, title: "Incorrect username or password.");
        if (verified == PasswordVerificationResult.SuccessRehashNeeded)
            user.PasswordHash = passwords.HashPassword(user, input.Password);

        // Each successful login replaces the previous key. Only this response contains the new key.
        var key = ApiKeyHandler.IssueKey();
        user.ApiKeyHash = ApiKeyHandler.Digest(key);
        db.SaveChanges();
        return Ok(new LoginResult(user.UserName, key));
    }
}
