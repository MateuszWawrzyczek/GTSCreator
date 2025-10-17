using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rozklady.Data;
using Rozklady.Models;
using System.Xml.Linq;
using System.Text.Json;
using System.Globalization;
using System.Net;
using System.Threading;

[ApiController]
[Route("api/[controller]")]
public class VehiclesController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<VehiclesController> _logger;
    private readonly IDbContextFactory<RozkladyContext> _contextFactory;
    private readonly TripsHistoryService _tripsHistoryService;

    private static List<VehicleDto> _cache = new();
    private static DateTime _lastFetch = DateTime.MinValue;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(5);
    private static readonly SemaphoreSlim _semaphore = new(5); 


    public VehiclesController(
        IDbContextFactory<RozkladyContext> contextFactory,
        HttpClient httpClient,
        ILogger<VehiclesController> logger,
        TripsHistoryService tripsHistoryService)
    {
        _contextFactory = contextFactory;
        _httpClient = httpClient;
        _logger = logger;
        _tripsHistoryService = tripsHistoryService;
        _httpClient.Timeout = TimeSpan.FromSeconds(2);

    }

    [HttpGet("vehiclePositions")]
    public async Task<IActionResult> GetVehiclePositionsJson()
    {
        if (DateTime.UtcNow - _lastFetch < CacheDuration)
            return Ok(_cache);

        await using var db = _contextFactory.CreateDbContext();
        

        var allDbVehicles = await db.Vehicles
            .AsNoTracking()
            .ToDictionaryAsync(v => v.FleetNumber);

        var mzkRoutes = await db.Routes
            .Where(r => r.FeedId == "MZK")
            .OrderBy(r => r.RouteShortName)
            .Select(r => r.RouteShortName.Trim())
            .ToListAsync();

        var kmrRoutes = await db.Routes
            .Where(r => r.FeedId == "KMR")
            .OrderBy(r => r.RouteShortName)
            .Select(r => r.RouteShortName.Trim())
            .ToListAsync();

        var mzkTasks = mzkRoutes.Select(routeId =>
            FetchVehiclesAsync(routeId, "MZK",
                $"http://e-biletmzkjastrzebie.com:8081/Home/CNR_GetVehicles?r={routeId}&d=&nb=", 
                allDbVehicles));

        var kmrTasks = kmrRoutes.Select(routeId =>
            FetchVehiclesAsync(routeId, "KMR",
                $"https://rozklad.km.rybnik.pl/Home/CNR_GetVehicles?r={routeId}&d=&nb=&krs=", 
                allDbVehicles));

        var allVehicles = (await Task.WhenAll(mzkTasks.Concat(kmrTasks)))
            .SelectMany(v => v)
            .ToList();

        _cache = allVehicles;
        _lastFetch = DateTime.UtcNow;

        return Ok(allVehicles);
    }

    private async Task<List<VehicleDto>> FetchVehiclesAsync(
        string routeId,
        string feedId,
        string url,
        Dictionary<string, Vehicle> allDbVehicles)
    {
        await using var db = _contextFactory.CreateDbContext();

        await _semaphore.WaitAsync();
        try
        {
            var xmlString = await _httpClient.GetStringAsync(url);
            var xdoc = XDocument.Parse(xmlString);
            var pElements = xdoc.Descendants("p");

            var vehicles = new List<VehicleDto>();

            foreach (var p in pElements)
            {
                try
                {
                    var text = p.Value;
                    var row = JsonSerializer.Deserialize<List<object>>(text);
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
                                .FirstOrDefaultAsync();
                        }

                    var vehicleInfo = new VehicleDto
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
                    };

                    vehicles.Add(vehicleInfo);
                }
                catch 
                {
                    //_logger.LogWarning(ex, "Failed to parse vehicle row: {row}");

                    continue;
                }
            }

            //await _tripsHistoryService.ProcessVehiclePositions(vehicles);
            return vehicles;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching vehicles for route {routeId} from {url}", routeId, url);
        
            return new List<VehicleDto>();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private string ParseString(object obj)
    {
        var str = obj switch
        {
            JsonElement je => je.ToString(),
            _ => obj?.ToString() ?? ""
        };
        return WebUtility.HtmlDecode(str);
    }
}
