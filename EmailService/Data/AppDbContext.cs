using Microsoft.EntityFrameworkCore;
using ACI.Entities;

namespace ACI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        
        public DbSet<User> Users { get; set; }
        public DbSet<EmailLog> EmailLogs { get; set; }
    }
}
