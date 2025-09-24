using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rozklady.Data;
using Rozklady.Models;
using System.Xml.Linq;
using System.Text.Json;
using System.Globalization;


[ApiController]
[Route("api/[controller]")]
public class VehiclesController : ControllerBase
{
    private readonly RozkladyContext _context;
    private readonly HttpClient _httpClient;
    private readonly ILogger<VehiclesController> _logger;


    private static List<VehicleDto> _cache = new();
    private static DateTime _lastFetch = DateTime.MinValue;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(5);
    private readonly IDbContextFactory<RozkladyContext> _contextFactory;


    public VehiclesController(IDbContextFactory<RozkladyContext> contextFactory, HttpClient httpClient, ILogger<VehiclesController> logger)
    {
        _contextFactory = contextFactory;

        _httpClient = httpClient;
        _logger = logger;
    }

    [HttpGet("vehiclePositions")]
public async Task<IActionResult> GetVehiclePositionsJson()
{
    if (DateTime.UtcNow - _lastFetch < CacheDuration)
        return Ok(_cache);

    await using var db = _contextFactory.CreateDbContext();

    var routeIds = await db.Routes
        .Where(r => r.FeedId == "MZK")
        .OrderBy(r => r.RouteShortName)
        .Select(r => r.RouteShortName.Trim())
        .ToListAsync();

    var tasks = routeIds.Select(async routeId =>
    {
        try
        {
            var url = $"http://e-biletmzkjastrzebie.com:8081/Home/CNR_GetVehicles?r={routeId}&d=&nb=";
            var xmlString = await _httpClient.GetStringAsync(url);

            var xdoc = XDocument.Parse(xmlString);
            var pElements = xdoc.Descendants("p");

            var vehicles = new List<VehicleDto>();

            foreach (var p in pElements)
            {
                var text = p.Value;
                try
                {
                    var row = JsonSerializer.Deserialize<List<object>>(text);
                    if (row != null)
                    {
                        var fleet = ParseString(row[0]);
                        double.TryParse(ParseString(row[9]), System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out var lon);
                        double.TryParse(ParseString(row[10]), System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture, out var lat);
                        var direction = ParseString(row[25]) == "" ? ParseString(row[26]) : ParseString(row[25]);
                        var delay = ParseString(row[14]);
                        await using var db2 = _contextFactory.CreateDbContext();

                        var vehicleInfo = await db2.Vehicles
                            .Where(v => v.FleetNumber == fleet)
                            .Select(v => new VehicleDto
                            {
                                FleetNumber = v.FleetNumber,
                                RouteId = routeId,
                                Model = v.Model,
                                AirConditioning = v.AirConditioning,
                                Longitude = lon,
                                Latitude = lat,
                                DirectionName = direction,
                                Delay = delay
                            })
                            .FirstOrDefaultAsync();

                        if (vehicleInfo != null)
                            vehicles.Add(vehicleInfo);
                    }
                }
                catch
                {
                    continue;
                }
            }

            return vehicles;
        }
        catch
        {
            return new List<VehicleDto>();
        }
    });

    var allVehicles = (await Task.WhenAll(tasks)).SelectMany(v => v).ToList();

    _cache = allVehicles;
    _lastFetch = DateTime.UtcNow;

    return Ok(allVehicles);
}
    private string ParseString(object obj)
    {
        return obj switch
        {
            JsonElement je => je.ToString(),
            _ => obj?.ToString() ?? ""
        };
    }
}

