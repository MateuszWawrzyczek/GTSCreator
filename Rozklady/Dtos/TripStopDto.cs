public class TripStopDto
{
    public string FeedId { get; set; } 
    public string StopId { get; set; }
    public string TripId { get; set; } 
    public string StopName { get; set; } 
    public double StopLat { get; set; } 
    public double StopLon { get; set; } 
    public string RouteShortName { get; set; }
    public TimeSpan? DepartureTime { get; set; } 
}
