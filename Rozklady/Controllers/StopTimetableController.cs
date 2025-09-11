using Microsoft.AspNetCore.Mvc;
using Rozklady.Data;
using Rozklady.Models;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class StopTimetableController : ControllerBase
{
    private readonly RozkladyContext _context;

    public StopTimetableController(RozkladyContext context)
    {
        _context = context;
    }
    [HttpGet("stop/{feedId}/{stopId}/timetable")]
    public async Task<ActionResult<StopScheduleDto>> GetStopTimetable(string feedId, string stopId)
    {
        var query = from t in _context.Trips
                    join st in _context.StopTimes on new { t.TripId, t.FeedId } equals new { st.TripId, st.FeedId }
                    join r in _context.Routes on new { t.RouteId, t.FeedId } equals new { r.RouteId, r.FeedId }
                    join s in _context.ServiceTypes on t.ServiceId equals s.ServiceId
                    join stop in _context.Stops on new { st.StopId, st.FeedId } equals new { stop.StopId, stop.FeedId }
                    where st.StopId == stopId && st.FeedId == feedId
                    select new
                    {
                        st.StopId,
                        stop.StopName,
                        s.DayType,
                        RouteFeedId = r.FeedId,
                        r.RouteShortName,
                        st.DepartureTime,
                        t.TripId,
                        t.FeedId
                    };

        var data = await query.ToListAsync();

        if (!data.Any())
            return NotFound($"Brak rozkładu dla przystanku {stopId}");

        var result = new StopScheduleDto
        {
            StopId = stopId,
            StopName = data.First().StopName,
            Lines = data
                .GroupBy(x => x.RouteShortName)
                .OrderBy(g => g.Key)
                .Select(gr => new RouteScheduleDto
                {
                    Route = gr.Key,
                    FeedId = gr.First().RouteFeedId,
                    Days = gr
                        .GroupBy(x => x.DayType)
                        .ToDictionary(
                            d => d.Key,
                            d => d
                                .OrderBy(x => x.DepartureTime)
                                .Select(x => new StopDepartureDto
                                {
                                    TripId = x.TripId,
                                    FeedId = x.FeedId,
                                    Time = x.DepartureTime.HasValue
                                        ? x.DepartureTime.Value.ToString(@"hh\:mm")
                                        : "??:??"
                                })
                                .ToList()
                        )
                })
                .ToList()
        };
        return Ok(result);
    }
}