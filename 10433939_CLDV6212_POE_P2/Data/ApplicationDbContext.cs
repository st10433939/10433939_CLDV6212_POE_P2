using Microsoft.EntityFrameworkCore;

namespace _10433939_CLDV6212_POE_P2.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<_10433939_CLDV6212_POE_P2.Models.Customer> Customer { get; set; } = default!;
    }
}

