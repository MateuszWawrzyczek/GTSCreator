public class TripHistory
{
    public string TripId { get; set; } = string.Empty;
    public string FleetNumber { get; set; } = string.Empty;
    public string RouteId { get; set; } = string.Empty;
    public string Direction { get; set; } = string.Empty;
    public DateTime? PlannedStartTime { get; set; }
    public DateTime? PlannedEndTime { get; set; }
    public DateTime? ActualStartTime { get; set; }
    public DateTime? ActualEndTime { get; set; }
}
