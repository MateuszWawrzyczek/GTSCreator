public class ScrapedData
{
    public Customer Provider { get; set; } = default!;
    public List<RawStop> Stops { get; set; } = new();
    public List<TripDetails> Trips { get; set; } = new();
    public Dictionary<string, HashSet<string>> TripCalendar { get; set; } = new();
}
