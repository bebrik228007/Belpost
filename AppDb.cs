using Microsoft.EntityFrameworkCore;

namespace Belpost.Auth
{
    public class AppDb : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<PasswordHistory> PasswordHistory { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=belpost.db");
        }
    }
}
