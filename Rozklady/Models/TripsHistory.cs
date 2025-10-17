using System.ComponentModel.DataAnnotations.Schema;

[Table("trips_history")]
public class TripsHistory
{
    //public int Id { get; set; }
    [Column("feed_id")]
    public string FeedId { get; set; } = null!;
    [Column("trip_id")]
    public string TripId { get; set; } = null!;
    [Column("route_id")]
    public string RouteId { get; set; } = null!;
    [Column("direction")]
    public string Direction { get; set; } = null!;
    [Column("fleet_number")]
    public string FleetNumber { get; set; } = null!;
    [Column("planned_start_time")]
    public DateTime? PlannedStartTime { get; set; }
    [Column("planned_end_time")]
    public DateTime? PlannedEndTime { get; set; }
    [Column("actual_start_time")]
    public DateTime? ActualStartTime { get; set; }
    [Column("actual_end_time")]
    public DateTime? ActualEndTime { get; set; }
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
