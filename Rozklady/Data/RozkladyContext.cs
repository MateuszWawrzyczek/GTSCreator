using Microsoft.EntityFrameworkCore;
using Rozklady.Models;

namespace Rozklady.Data;

public class RozkladyContext : DbContext
{
    public RozkladyContext(DbContextOptions<RozkladyContext> options) : base(options) { }

    public DbSet<Stop> Stops { get; set; } = null!;
    public DbSet<TransitRoute> Routes { get; set; } = null!;
    public DbSet<Trip> Trips { get; set; } = null!;
    public DbSet<StopTime> StopTimes { get; set; } = null!;
    public DbSet<Calendar> Calendars { get; set; } = null!;
    public DbSet<CalendarDates> CalendarDates { get; set; } = null!;
    public DbSet<ServiceType> ServiceTypes { get; set; } = null!;
    public DbSet<Vehicle> Vehicles { get; set; } = null!;
    public DbSet<DayType> DayTypes { get; set; } = null!;
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Stop>().HasKey(s => new { s.FeedId, s.StopId });
        modelBuilder.Entity<TransitRoute>().HasKey(r => new { r.FeedId, r.RouteId });
        modelBuilder.Entity<Trip>().HasKey(t => new { t.FeedId, t.TripId });
        modelBuilder.Entity<StopTime>().HasKey(st => new { st.FeedId, st.TripId, st.StopId, st.StopSequence });
        modelBuilder.Entity<Calendar>().HasKey(c => new { c.FeedId, c.ServiceId });
        modelBuilder.Entity<CalendarDates>().HasKey(cd => new { cd.FeedId, cd.ServiceId, cd.Date });
        modelBuilder.Entity<ServiceType>().HasKey(sr => new { sr.ServiceId });
        modelBuilder.Entity<Vehicle>().HasKey(v => new { v.FleetNumber });
        modelBuilder.Entity<DayType>().HasKey(d => new { d.Date });

        modelBuilder.Entity<Trip>()
            .HasOne(t => t.Route)
            .WithMany(r => r.Trips)
            .HasForeignKey(t => new { t.FeedId, t.RouteId });

        modelBuilder.Entity<StopTime>()
            .HasOne(st => st.Trip)
            .WithMany(t => t.StopTimes)
            .HasForeignKey(st => new { st.FeedId, st.TripId });

        modelBuilder.Entity<StopTime>()
            .HasOne(st => st.Stop)
            .WithMany(s => s.StopTimes)
            .HasForeignKey(st => new { st.FeedId, st.StopId });
    }
}
