using Microsoft.EntityFrameworkCore;
using WorkshopAPI.Models;

namespace WorkshopsSite.Data
{
    public class CatalogDB(DbContextOptions<CatalogDB> options) : DbContext(options)
    {
        public DbSet<Workshop> Workshops => Set<Workshop>();
        public DbSet<CampusUser> Users => Set<CampusUser>();
        public DbSet<ChatMessage> Messages => Set<ChatMessage>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Workshop>().HasData(WorkshopSeed.Rows);
            var users = modelBuilder.Entity<CampusUser>();
            users.Property(u => u.UserName).UseCollation("NOCASE");
            users.HasIndex(u => u.UserName).IsUnique();
            users.HasIndex(u => u.ApiKeyHash).IsUnique();
        }
    }
}
