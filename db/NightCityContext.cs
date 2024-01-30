using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NityCityWeb.Models;

namespace NityCityWeb.db
{
    public class NightCityContext : DbContext
    {
        private readonly IConfiguration _configuration;

        public DbSet<District> Districts { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Pricing> Pricings { get; set; }

        public NightCityContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_configuration.GetConnectionString("DefaultConnection"));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Pricing>().ToTable("Pricing");

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany() // or specify the collection property if there is one
                .HasForeignKey(b => b.UserId);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.District)
                .WithMany() // or specify the collection property if there is one
                .HasForeignKey(b => b.DistrictId);
        }

    }
}
