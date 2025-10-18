using CsvHelper;
using System.Globalization;
using System.IO.Compression;
using System.Collections.Concurrent;

public class GtfsGenerator
{
    string FormatStopName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return name;

        if (name == name.ToUpper())
        {
            var textInfo = new CultureInfo("pl-PL", false).TextInfo;
            return textInfo.ToTitleCase(name.ToLower());
        }

        return name;
    }

    public async Task<byte[]> GenerateGtfsAsync(ScrapedData data)
    {
        using var memStream = new MemoryStream();

        using (var archive = new ZipArchive(memStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            await WriteCsvAsync(archive, "agency.txt", new[]
            {
                new {
                    agency_id = data.Provider.Prefix,
                    agency_name = data.Provider.Name,
                    agency_url = $"https://{data.Provider.Prefix}.{data.Provider.Domain}",
                    agency_timezone = "Europe/Warsaw",
                    agency_lang = "pl"
                }
            });

            var routes = new List<object>();
            var trips = new List<object>();
            var stopTimes = new List<object>();
            var calendarDates = new List<object>();
            var calendarDatesSet = new ConcurrentDictionary<(string serviceId, string date), byte>();
            var usedStops = new HashSet<string>();

            foreach (var trip in data.Trips)
            {
                var routeId = trip.Line.Name;
                if (!routes.Any(r => ((dynamic)r).route_id == routeId))
                {
                    routes.Add(new
                    {
                        route_id = routeId,
                        agency_id = data.Provider.Prefix,
                        route_short_name = routeId,
                        route_type = 3
                    });
                }

                trips.Add(new
                {
                    route_id = routeId,
                    service_id = trip.TripId,
                    trip_id = trip.TripId,
                    trip_headsign = trip.Direction
                });

   

                 if (data.TripCalendar.TryGetValue(trip.TripId.ToString(), out var dates))
                {
                    foreach (var d in dates)
                    {
                        if (calendarDatesSet.TryAdd((trip.TripId.ToString(), d), 0))
                        {
                        }
                    }
                }

                //int lastDepartureSeconds = -1;
                for (int i = 0; i < trip.Times.Count; i++)
                {
                    var st = trip.Times[i];
                    //int departureSeconds = TimeUtils.TimeToSeconds(st.DepartureTime);

                    //if (departureSeconds < lastDepartureSeconds)
                        //departureSeconds += 24 * 3600;

                    //lastDepartureSeconds = departureSeconds;
                    usedStops.Add(st.PlaceId);

                    stopTimes.Add(new
                    {
                        trip_id = trip.TripId,
                        arrival_time = st.DepartureTime + ":00",
                        departure_time = st.DepartureTime + ":00",
                        stop_id = st.PlaceId,
                        stop_sequence = i + 1
                    });
                }
            }
            calendarDates = calendarDatesSet.Keys
                .Select(x => (object)new { service_id = x.serviceId, date = x.date, exception_type = 1 })
                .ToList();

            // --- stops.txt ---
            var filteredStops = data.Stops
                .Select(s => new
                {
                    stop_id = s.Id,
                    stop_code = s.Code,
                    stop_name = FormatStopName(s.Name),
                    stop_lon = s.Lon,
                    stop_lat = s.Lat
                });
            
            var first = stopTimes.FirstOrDefault();
            Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(first));

            await WriteCsvAsync(archive, "stops.txt", filteredStops);
            await WriteCsvAsync(archive, "routes.txt", routes);
            await WriteCsvAsync(archive, "trips.txt", trips);
            await WriteCsvAsync(archive, "stop_times.txt", stopTimes);
            await WriteCsvAsync(archive, "calendar_dates.txt", calendarDates);
            // Console.WriteLine($"Generated: routes={routes.Count}, trips={trips.Count}, stop_times={stopTimes.Count}, calendar_dates={calendarDates.Count}, stops={usedStops.Count}");

        }

        memStream.Position = 0;
        return memStream.ToArray();
    }

    private static async Task WriteCsvAsync<T>(ZipArchive archive, string fileName, IEnumerable<T> records)
    {
        var entry = archive.CreateEntry(fileName, CompressionLevel.Optimal);
        await using var stream = entry.Open();
        await using var writer = new StreamWriter(stream);
        await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        await csv.WriteRecordsAsync(records);
        await writer.FlushAsync(); 
    }
}
