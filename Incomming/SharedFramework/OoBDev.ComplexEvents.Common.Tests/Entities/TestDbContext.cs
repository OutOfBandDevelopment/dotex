using Microsoft.EntityFrameworkCore;

namespace OoBDev.ComplexEvents.Common.Tests.Entities
{
    public class TestDbContext : DbContext
    {
        protected TestDbContext()
        {
            this.OnConfiguring(new DbContextOptionsBuilder()
                .EnableSensitiveDataLogging()
                );
        }

        public TestDbContext(
            DbContextOptions<TestDbContext> options
            ) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
    }
}
