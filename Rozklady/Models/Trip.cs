using System.ComponentModel.DataAnnotations.Schema;
namespace Rozklady.Models;

[Table("trips")]
public class Trip
{
    [Column("feed_id")]
    public string FeedId { get; set; } = null!;

    [Column("trip_id")]
    public string TripId { get; set; } = null!;

    [Column("route_id")]
    public string RouteId { get; set; } = null!; 

    [Column("service_id")]
    public string ServiceId { get; set; } = null!;

    [Column("trip_headsign")]
    public string? TripHeadsign { get; set; }

    [Column("direction_id")]
    public int DirectionId { get; set; }

    public TransitRoute? Route { get; set; }
    public ICollection<StopTime> StopTimes { get; set; } = new List<StopTime>();
}
