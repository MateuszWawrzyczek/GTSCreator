using System.ComponentModel.DataAnnotations.Schema;

[Table("trips_history")]
public class TripsHistory
{
    [Column("id")]
    public int Id { get; set; }
    [Column("feed_id")]
    public string FeedId { get; set; } = null!;
    [Column("trip_id")]
    public string TripId { get; set; } = null!;
    [Column("route_id")]
    public string RouteId { get; set; } = null!;
    [Column("direction")]
    public string? Direction { get; set; } = null!;
    [Column("fleet_number")]
    public string FleetNumber { get; set; } = null!;
    [Column("planned_start_time", TypeName = "timestamp without time zone")]
    public DateTime? PlannedStartTime { get; set; }
    [Column("planned_end_time", TypeName = "timestamp without time zone")]
    public DateTime? PlannedEndTime { get; set; }
    [Column("actual_start_time", TypeName = "timestamp without time zone")]
    public DateTime? ActualStartTime { get; set; }
    [Column("actual_end_time", TypeName = "timestamp without time zone")]
    public DateTime? ActualEndTime { get; set; }
    [Column("created_at", TypeName = "timestamp without time zone")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
