using Microsoft.EntityFrameworkCore;
using ShengTaOrderListing.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ShengTaOrderListing.Services
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>()
                .Property(c => c.City)
                .HasConversion(
                    v => v.ToString(),                                   // 保存到数据库时 → string
                    v => (CityValue)Enum.Parse(typeof(CityValue), v)     // 从数据库读出时 → enum
                );

            base.OnModelCreating(modelBuilder);
        }
    }
}