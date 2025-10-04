using System.Text.Json.Serialization;

public class DeparturesResponse
{
    [JsonPropertyName("departures")]
    public List<ApiDeparture> Departures { get; set; } = new();
}

public class ApiDeparture
{
    [JsonPropertyName("departure")]
    public int Departure { get; set; }  // czas w sekundach od północy

    [JsonPropertyName("trip_id")]
    public long TripId { get; set; }

    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("platform")]
    public string Platform { get; set; } = "";
}
