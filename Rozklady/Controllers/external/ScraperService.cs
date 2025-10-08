using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Concurrent;


public class ScraperService
{
    private readonly IKiedyPrzyjedzieClient _client;
    private const int Concurrency = 5;

    public ScraperService(IKiedyPrzyjedzieClient client)
    {
        _client = client;
    }

    public async Task<ScrapedData?> RunScrapingPipelineAsync(Customer provider)
    {
        // 1. Pobierz przystanki
        var allStops = await _client.GetStopsAsync(provider.Prefix);
        if (!allStops.Any()) return null;
        Console.WriteLine($"Stops: {allStops.Count}, ");


        // 2. TripCalendar
        var tripCalendar = new ConcurrentDictionary<string, ConcurrentBag<string>>();

        var dates = TimeUtils.GetNextNDays(8);

        var stopTasks = allStops.SelectMany(stop => dates.Select(date => new { stop, date }));
        var throttler = new SemaphoreSlim(Concurrency);

        var tasks = stopTasks.Select(async sd =>
        {
            await throttler.WaitAsync();
            try
            {
                var departures = await _client.GetDeparturesAsync(provider.Prefix, sd.stop.Id, sd.date);
                //Console.WriteLine($"Fetched {departures} aaaaaaaaaaaa ");
                foreach (var dep in departures)
{
    var key = dep.TripId.ToString();
    var value = sd.date.Replace("-", "");

    tripCalendar.AddOrUpdate(
        key,
        _ => new ConcurrentBag<string> { value },
        (_, existing) =>
        {
            // Sprawdź, czy data już jest, żeby nie dublować
            if (!existing.Contains(value))
                existing.Add(value);
            return existing;
        });
}

            }
            finally { throttler.Release(); }
        });

        await Task.WhenAll(tasks);

        // 3. Pobierz szczegóły kursów – ignorujemy 404
        var allTripsDetails = new List<TripDetails>();
        foreach (var tripId in tripCalendar.Keys)
        {
            try
            {
                var trip = await _client.GetTripDetailsAsync(provider.Prefix, tripId);

                if (trip != null)
                {
                    trip.TripId = tripId;
                    allTripsDetails.Add(trip);
                }
                //Console.WriteLine($"Fetched trip {tripId}, total now {allTripsDetails}");
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // Pomijamy kursy, które zwracają 404
                continue;
            }
        }
        Console.WriteLine($"Stops: {allStops.Count}, Trips: {allTripsDetails.Count}, TripCalendar: {tripCalendar.Count}");
        var tripCalendarFinal = tripCalendar.ToDictionary(
            kvp => kvp.Key,
            kvp => new HashSet<string>(kvp.Value)
        );

        return new ScrapedData
        {
            Provider = provider,
            Stops = allStops,
            TripCalendar = tripCalendarFinal,
            Trips = allTripsDetails
        };
    }
}
