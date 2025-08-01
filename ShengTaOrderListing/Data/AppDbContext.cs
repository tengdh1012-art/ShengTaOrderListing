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
            modelBuilder.Entity<Customer>().ToTable("Customers");
        }
    }
}