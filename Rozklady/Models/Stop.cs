using System.ComponentModel.DataAnnotations.Schema;
namespace Rozklady.Models;

[Table("stops")]
public class Stop
{
    [Column("feed_id")]
    public required string FeedId { get; set; }

    [Column("stop_id")]
    public required string StopId { get; set; }

    [Column("stop_name")]
    public required string StopName { get; set; }

    [Column("stop_code")]
    public string? StopCode { get; set; }

    [Column("stop_lat")]
    public double StopLat { get; set; }

    [Column("stop_lon")]
    public double StopLon { get; set; }

    public ICollection<StopTime> StopTimes { get; set; } = new List<StopTime>();
}