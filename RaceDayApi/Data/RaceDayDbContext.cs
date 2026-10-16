using Microsoft.EntityFrameworkCore;
using RaceDayApi.Models;

namespace RaceDayApi.Data
{
    public class RaceDayDbContext : DbContext
    {
        public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : base(options)
        {
        }

        // a least one databse set per table
        public DbSet<User> Users => Set<User>();
        public DbSet<Event> Events => Set<Event>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<WeatherForecast> WeatherForecasts => Set<WeatherForecast>();
        public DbSet<RaceRoute> Routes => Set<RaceRoute>();
        public DbSet<Registration> Registrations => Set<Registration>();
        public DbSet<Result> Results => Set<Result>();
        public DbSet<Payment> Payments => Set<Payment>();

        // makes every string a varchar 
        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<string>().AreUnicode(false);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // DEFAULT GETDATE() on the two date columns
            modelBuilder.Entity<Registration>().Property(r => r.RegistrationDate).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<Payment>().Property(p => p.PaymentDate).HasDefaultValueSql("GETDATE()");

            // Without this the  SQL Server throws a "multiple cascade paths" error when we update the database.
            foreach (var fk in modelBuilder.Model.GetEntityTypes().SelectMany(t => t.GetForeignKeys()))
            {
                fk.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }
    }
}
