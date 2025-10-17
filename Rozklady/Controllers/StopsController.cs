using Microsoft.AspNetCore.Mvc;
using Rozklady.Data;
using Rozklady.Models;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class StopsController : ControllerBase
{
    private readonly RozkladyContext _context;

    public StopsController(RozkladyContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<StopWithRoutesDto>>> GetAllStopsWithRoutes()
    {
        var stops = await _context.Stops
            .Select(s => new StopWithRoutesDto
            {
                FeedId = s.FeedId,
                StopId = s.StopId,
                StopName = s.StopName,
                StopCode = s.StopCode,
                StopLat = s.StopLat,
                StopLon = s.StopLon

            })
            .OrderBy(s => s.StopName)
            .ToListAsync();

        return Ok(stops);
    }
    
    [HttpGet("{feedId}/{stopId}/routes")]
    public async Task<ActionResult<IEnumerable<RouteDto>>> GetRoutesForStop(string feedId, string stopId)
    {
        var stop = await _context.Stops
            .Include(s => s.StopTimes)
                .ThenInclude(st => st.Trip)
                .ThenInclude(t => t.Route)
            .FirstOrDefaultAsync(s => s.FeedId == feedId && s.StopId == stopId);

        if (stop == null)
        {
            return NotFound();
        }

        var routes = (stop.StopTimes ?? new List<StopTime>())
            .Where(st => st.Trip?.Route != null)
            .Select(st => st.Trip!.Route!)
            .Distinct()
            .Select(r => new RouteDto
            {
                FeedId = r.FeedId ?? "",
                RouteId = r.RouteId ?? "",
                RouteShortName = r.RouteShortName ?? ""
            })
            .OrderBy(r => r.RouteShortName)
            .ToList();

        return Ok(routes);
    }

}


