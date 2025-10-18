using Microsoft.EntityFrameworkCore;
using Rozklady.Data;
using Rozklady.Models;
using System.Xml.Linq;
using System.Text.Json;
using System.Globalization;
using System.Net;

public class RealTimeVehiclesService : BackgroundService
{
    private readonly IDbContextFactory<RozkladyContext> _contextFactory;
    private readonly ILogger<RealTimeVehiclesService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    private static readonly SemaphoreSlim _semaphore = new(5);
    private static List<VehicleDto> _cache = new();
    private static DateTime _lastUpdate = DateTime.MinValue;

    private static readonly TimeSpan UpdateInterval = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan HttpTimeout = TimeSpan.FromSeconds(2);

    public RealTimeVehiclesService(
        IDbContextFactory<RozkladyContext> contextFactory,
        ILogger<RealTimeVehiclesService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _contextFactory = contextFactory;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public static List<VehicleDto> GetCache() => _cache;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Vehicle background service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await UpdateVehiclesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating vehicles.");
            }

            await Task.Delay(UpdateInterval, stoppingToken);
        }

        _logger.LogInformation("Vehicle background service stopped.");
    }

    private async Task UpdateVehiclesAsync(CancellationToken ct)
    {
        await using var db = _contextFactory.CreateDbContext();

        var allDbVehicles = await db.Vehicles
            .AsNoTracking()
            .ToDictionaryAsync(v => v.FleetNumber, ct);

        var mzkRoutes = await db.Routes
            .Where(r => r.FeedId == "MZK")
            .Select(r => r.RouteShortName.Trim())
            .ToListAsync(ct);

        var kmrRoutes = await db.Routes
            .Where(r => r.FeedId == "KMR")
            .Select(r => r.RouteShortName.Trim())
            .ToListAsync(ct);

        var client = _httpClientFactory.CreateClient();
        client.Timeout = HttpTimeout;

        var mzkTasks = mzkRoutes.Select(routeId =>
            FetchVehiclesAsync(routeId, "MZK", 
                $"http://bilet.mzkjastrzebie.com:8081/Home/CNR_GetVehicles?r={routeId}&d=&nb=", 
                allDbVehicles, client, ct));

        var kmrTasks = kmrRoutes.Select(routeId =>
            FetchVehiclesAsync(routeId, "KMR", 
                $"https://rozklad.km.rybnik.pl/Home/CNR_GetVehicles?r={routeId}&d=&nb=&krs=", 
                allDbVehicles, client, ct));

        var results = await Task.WhenAll(mzkTasks.Concat(kmrTasks));

        var allVehicles = results.SelectMany(v => v ?? Enumerable.Empty<VehicleDto>()).ToList();

        if (allVehicles.Count > 0)
        {
            _cache = allVehicles;
            _lastUpdate = DateTime.UtcNow;
        }
        else
        {
            _logger.LogWarning("No vehicles fetched in this update — keeping previous cache.");
        }
    }

    private async Task<List<VehicleDto>?> FetchVehiclesAsync(
        string routeId,
        string feedId,
        string url,
        Dictionary<string, Vehicle> allDbVehicles,
        HttpClient client,
        CancellationToken ct)
    {
        await using var db = _contextFactory.CreateDbContext();

        await _semaphore.WaitAsync(ct);
        try
        {
            var xmlString = await client.GetStringAsync(url, ct);
            var xdoc = XDocument.Parse(xmlString);
            var vehicles = new List<VehicleDto>();

            foreach (var p in xdoc.Descendants("p"))
            {
                try
                {
                    var row = JsonSerializer.Deserialize<List<object>>(p.Value);
                    if (row == null) continue;

                    var fleet = ParseString(row[0]);
                    double.TryParse(ParseString(row[9]), NumberStyles.Any, CultureInfo.InvariantCulture, out var lon);
                    double.TryParse(ParseString(row[10]), NumberStyles.Any, CultureInfo.InvariantCulture, out var lat);
                    var direction = ParseString(row[25]) == "" ? ParseString(row[26]) : ParseString(row[25]);
                    var delay = ParseString(row[14]);
                    var tripId = ParseString(row[5]) == "0" ? ParseString(row[17]) : ParseString(row[5]);
                    var onTrip = ParseString(row[5]) != "0";

                    allDbVehicles.TryGetValue(fleet, out var dbVehicle);

                    string? blockId = null;
                    if (!string.IsNullOrEmpty(tripId))
                    {
                        blockId = await db.Trips
                            .Where(t => t.FeedId == feedId && t.TripId.EndsWith("_" + tripId))
                            .Select(t => t.BlockId)
                            .FirstOrDefaultAsync(ct);
                    }

                    vehicles.Add(new VehicleDto
                    {
                        FleetNumber = dbVehicle?.FleetNumber ?? fleet,
                        RouteId = routeId,
                        TripId = tripId,
                        FeedId = feedId,
                        Model = dbVehicle?.Model,
                        AirConditioning = dbVehicle?.AirConditioning ?? false,
                        Longitude = lon,
                        Latitude = lat,
                        DirectionName = direction,
                        Delay = delay,
                        OnTrip = onTrip,
                        BlockId = blockId
                    });
                }
                catch { continue; }
            }

            return vehicles;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"Failed to fetch vehicles for {feedId} route {routeId}");
            return null;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private static string ParseString(object obj)
    {
        var str = obj switch
        {
            JsonElement je => je.ToString(),
            _ => obj?.ToString() ?? ""
        };
        return WebUtility.HtmlDecode(str);
    }
}
