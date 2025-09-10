using Microsoft.EntityFrameworkCore;
using HoteleriaGes.Models;

namespace HoteleriaGes.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Hotel> Hoteles { get; set; }
        // ...otros DbSet si los necesitas...
    }
}
