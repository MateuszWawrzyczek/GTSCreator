using System.Collections.Generic;
using System.Text.Json.Serialization;

public class TripDetails
{
    [JsonPropertyName("trip_id")]
    public string TripId { get; set; } = default!;

    [JsonPropertyName("direction")]
    public string Direction { get; set; } = default!;

    [JsonPropertyName("line")]
    public LineInfo Line { get; set; } = default!;

    [JsonPropertyName("times")]
    public List<StopTimeDetail> Times { get; set; } = new();
}

public class StopTimeDetail
{
    [JsonPropertyName("place_id")] // lub "place_id" jeśli API tak nazywa
    public string PlaceId { get; set; } = default!;

    [JsonPropertyName("departure_time")]
    public string DepartureTime { get; set; } = default!; // np. "14:35:00"
}

public class LineInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;
}
