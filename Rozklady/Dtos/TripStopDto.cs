public class TripStopDto
{
    public required string FeedId { get; set; } 
    public required string StopId { get; set; }
    public required string TripId { get; set; } 
    public required string StopName { get; set; } 
    public double StopLat { get; set; } 
    public double StopLon { get; set; } 
    public required string RouteShortName { get; set; }
    public TimeSpan? DepartureTime { get; set; } 
}
