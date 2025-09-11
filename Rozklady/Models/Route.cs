using System.ComponentModel.DataAnnotations.Schema;
namespace Rozklady.Models;

[Table("routes")]
public class TransitRoute
{
    [Column("feed_id")]
    public string FeedId { get; set; } = null!;

    [Column("route_id")]
    public string RouteId { get; set; } = null!;

    [Column("route_short_name")]
    public string RouteShortName { get; set; } = null!;

    [Column("route_long_name")]
    public string? RouteLongName { get; set; }

    [Column("route_type")]
    public int RouteType { get; set; }

    public ICollection<Trip> Trips { get; set; } = new List<Trip>();
}
