using Microsoft.EntityFrameworkCore;

namespace AISmartHub.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Models.AIInteraction> AIInteractions { get; set; }
    }
}
