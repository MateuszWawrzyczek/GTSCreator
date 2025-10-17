public class ActiveTripDto
{
    public string FeedId { get; set; } = null!;
    public string RouteId { get; set; } = null!;
    public string TripId { get; set; } = null!;
    public string FleetNumber { get; set; } = null!;
    public DateTime FirstSeen { get; set; }
    public DateTime LastSeen { get; set; }
    public DateTime? PlannedStartTime { get; set; }
    public DateTime? PlannedEndTime { get; set; }
    public string? DirectionName { get; set; }
    public TimeSpan? Delay { get; set; }
    public double? LastLat { get; set; }
    public double? LastLon { get; set; }
}

