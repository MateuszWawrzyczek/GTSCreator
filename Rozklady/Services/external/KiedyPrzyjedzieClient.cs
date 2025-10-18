using System.Text.Json;

public interface IKiedyPrzyjedzieClient
{
    Task<List<RawStop>> GetStopsAsync(string prefix);
    Task<List<ApiDeparture>> GetDeparturesAsync(string prefix, string stopId, string date);
    Task<TripDetails?> GetTripDetailsAsync(string prefix, string tripId);
}


public class KiedyPrzyjedzieClient : IKiedyPrzyjedzieClient
{
    private readonly HttpClient _httpClient;

    public KiedyPrzyjedzieClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<RawStop>> GetStopsAsync(string prefix)
    {
        var respStream = await _httpClient.GetStreamAsync($"https://{prefix}.kiedyprzyjedzie.pl/stops");
        using var doc = await JsonDocument.ParseAsync(respStream);

        var stopsList = new List<RawStop>();
        foreach (var stopArray in doc.RootElement.GetProperty("stops").EnumerateArray())
        {
            stopsList.Add(new RawStop
            {
                Id = stopArray[0].GetString()!,
                Code = stopArray[1].GetInt32().ToString(),
                Name = stopArray[2].GetString()!,
                Lon = stopArray[3].GetInt32() / 1_000_000.0,
                Lat = stopArray[4].GetInt32() / 1_000_000.0
            });
        }

        return stopsList;
    }

public async Task<List<ApiDeparture>> GetDeparturesAsync(string prefix, string stopId, string date)
{
    var url = $"https://{prefix}.kiedyprzyjedzie.pl/api/timetables/{stopId}?date={date}";
    var response = await _httpClient.GetAsync(url);
    var raw = await response.Content.ReadAsStringAsync();

    Console.WriteLine($"[DEBUG] Departures raw for stop={stopId} date={date}: ...");

    try
    {
        var resp = JsonSerializer.Deserialize<DeparturesResponse>(raw, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return resp?.Departures ?? new List<ApiDeparture>();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR] Failed to parse departures for stop={stopId}, date={date}: {ex.Message}");
        return new List<ApiDeparture>();
    }
}



    public async Task<TripDetails?> GetTripDetailsAsync(string prefix, string tripId)
    {

        var urls = new[]
        {
            $"https://{prefix}.kiedyprzyjedzie.pl/api/trip/{tripId}/0",
            $"https://{prefix}.kiedyprzyjedzie.pl/api/trip/{tripId}"
        };

        foreach (var url in urls)
        {
            Console.WriteLine($"Trip fetched: tripId={tripId}");

            try
            {
                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<TripDetails>();
                }
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    // spróbuj następny wariant
                    continue;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WARN] Nie udało się pobrać trip {tripId} z {url}: {ex.Message}");
            }
        }

        return null; // brak szczegółów dla tego kursu
    }

}



// DTO odpowiedzi API
public class StopsResponse { public List<RawStop> Stops { get; set; } = new(); }
//public class DeparturesResponse { public List<Departure> Departures { get; set; } = new(); }
