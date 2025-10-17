public class StopWithRoutesDto
{
    public required string StopId { get; set; }
    public required string FeedId { get; set; }
    public required string StopName { get; set; }
    public string? StopCode { get; set; }
    public double StopLat { get; set; }
    public double StopLon { get; set; }

    //public List<RouteDto> Routes { get; set; }
}


