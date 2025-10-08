public class StopScheduleDto
{
    public string StopId { get; set; } = "";
    public string StopName { get; set; } = "";
    public List<RouteScheduleDto> Lines { get; set; } = new();
}

public class RouteScheduleDto
{
    public string Route { get; set; } = "";
    public string FeedId { get; set; } = "";
    public Dictionary<string, List<StopDepartureDto>> Days { get; set; } = new();
}

public class StopDepartureDto
{
    public string TripId { get; set; } = "";
    public string FeedId { get; set; } = "";
    public string Time { get; set; } = "";
}
