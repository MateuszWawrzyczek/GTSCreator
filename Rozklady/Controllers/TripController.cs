using Microsoft.AspNetCore.Mvc;
using Rozklady.Data;
using Rozklady.Models;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class TripController : ControllerBase
{
    private readonly RozkladyContext _context;

    public TripController(RozkladyContext context)
    {
        _context = context;
    }

    [HttpGet("{feedId}/{tripId}/")]
    public async Task<ActionResult<IEnumerable<TripStopDto>>> GetTripInfo(string feedId, string tripId)
    {
        
        var tripDepartures = await (
        from st in _context.StopTimes
        join s in _context.Stops on new { st.StopId, st.FeedId } equals new { s.StopId, s.FeedId }
        join t in _context.Trips on new { st.TripId, st.FeedId } equals new { t.TripId, t.FeedId }
        join r in _context.Routes on new { t.RouteId, t.FeedId } equals new { r.RouteId, r.FeedId }
        where t.FeedId == feedId && t.TripId == tripId
        orderby st.StopSequence
        select new TripStopDto
        {
            FeedId = s.FeedId,
            TripId = st.TripId,
            StopId = s.StopId,
            StopName = s.StopName,
            StopLat = s.StopLat,
            StopLon = s.StopLon,
            RouteShortName = r.RouteShortName,
            DepartureTime = st.DepartureTime
        }
    ).ToListAsync();

        return Ok(tripDepartures);
    }   
}


