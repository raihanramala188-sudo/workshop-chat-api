using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WorkshopsSite.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi;
using WorkshopAPI.Models;
using WorkshopAPI.Security;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options => options.AddDocumentTransformer((document, _, _) =>
{
    document.Components ??= new();
    document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
    document.Components.SecuritySchemes[ApiKeyHandler.SchemeName] = new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey, In = ParameterLocation.Header, Name = ApiKeyHandler.HeaderName
    };
document.Paths["/api/workshops"].Operations![HttpMethod.Post].Security =
    [new() { [new OpenApiSecuritySchemeReference(ApiKeyHandler.SchemeName, document)] = [] }];
    document.Paths["/api/workshops/{id}"].Operations![HttpMethod.Put].Security =
    [new() { [new OpenApiSecuritySchemeReference(ApiKeyHandler.SchemeName, document)] = [] }];
    return Task.CompletedTask;
}));
builder.Services.AddScoped<IPasswordHasher<CampusUser>, PasswordHasher<CampusUser>>();
builder.Services.AddAuthentication(ApiKeyHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, ApiKeyHandler>(ApiKeyHandler.SchemeName, _ => { });
builder.Services.AddAuthorization();

var connection = new SqliteConnectionStringBuilder
{
    DataSource = Path.Combine(builder.Environment.ContentRootPath, "campus.db")
};

builder.Services.AddDbContext<CatalogDB>(options => options.UseSqlite(connection.ToString()));

var app = builder.Build();

// A fresh copy creates a local SQLite database from the six workshops in WorkshopSeed.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CatalogDB>();
    db.Database.EnsureCreated();
    // EnsureCreated does not upgrade an existing Day 4 catalogue. Add its new account
    // table and indexes in one transaction, preserving the existing workshop rows.
    using var upgrade = db.Database.BeginTransaction();
    db.Database.ExecuteSqlRaw("""
        CREATE TABLE IF NOT EXISTS Users (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            UserName TEXT COLLATE NOCASE NOT NULL,
            PasswordHash TEXT NOT NULL,
            ApiKeyHash TEXT NULL
        );
        CREATE UNIQUE INDEX IF NOT EXISTS IX_Users_UserName ON Users (UserName);
        CREATE UNIQUE INDEX IF NOT EXISTS IX_Users_ApiKeyHash ON Users (ApiKeyHash);
        CREATE TABLE IF NOT EXISTS Messages (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            WorkshopId INTEGER NULL,
            Sender TEXT NOT NULL,
            Text TEXT NOT NULL,
            CreatedAtUtc TEXT NOT NULL
        );
        """);
    upgrade.Commit();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
// Serve the single HTML page and its assets from the same origin as /api/workshops.
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();


