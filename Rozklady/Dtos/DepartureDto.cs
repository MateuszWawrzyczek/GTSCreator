public class StopDeparturesDto
{
    public string StopName { get; set; } = null!;
    public List<DepartureDto> Departures { get; set; } = new();
}

public class DepartureDto
{
    public string FeedId { get; set; } = null!;
    public string TripId { get; set; } = null!;
    public string StopId { get; set; } = null!;
    public string Headsign { get; set; } = null!;
    public string RouteShortName { get; set; } = null!;
    public TimeSpan DepartureTime { get; set; }
    public string? Delay { get; set; }
    public string? FleetNumber { get; set; }
    public bool OnTrip { get; set; }
}