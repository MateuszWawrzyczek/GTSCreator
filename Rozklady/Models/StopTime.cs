using System.ComponentModel.DataAnnotations.Schema;
namespace Rozklady.Models;

[Table("stop_times")]
public class StopTime
{
    [Column("feed_id")]
    public string FeedId { get; set; } = null!;

    [Column("trip_id")]
    public string TripId { get; set; } = null!;

    [Column("stop_id")]
    public string StopId { get; set; } = null!;

    [Column("stop_sequence")]
    public int StopSequence { get; set; }

    [Column("arrival_time")]
    public TimeSpan? ArrivalTime { get; set; }

    [Column("departure_time")]
    public TimeSpan? DepartureTime { get; set; }

    public Trip? Trip { get; set; }
    public Stop? Stop { get; set; }
}
