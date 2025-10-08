using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rozklady.Data;
using Rozklady.Models;

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
        var today = DateOnly.FromDateTime(DateTime.Now);
        var startDate = today;
        var endDate = today.AddDays(7);

        var upcomingDays = await _context.DayTypes
        .Where(d => d.Date >= today)
        .GroupBy(d => d.Type)
        .Select(g => new
        {
            Type = g.Key,
            Date = g.Min(d => d.Date)
        })
        .OrderBy(x => x.Date)
        .ToListAsync();

        var calendarEntries = await _context.CalendarDates
            .Where(c => c.FeedId == feedId && c.Date >= startDate )
            .ToListAsync();

        var dateMappings = (
            from d in upcomingDays
            join c in calendarEntries on d.Date equals c.Date into gj
            from c in gj.DefaultIfEmpty()
            select new
            {
                Date = d.Date,
                DayType = d.Type,
                ServiceId = c?.ServiceId
            }
        ).Where(x => x.ServiceId != null).ToList();

        IQueryable<dynamic> query;

        if (!dateMappings.Any())
        {
            query =
                from t in _context.Trips
                join st in _context.StopTimes on new { t.TripId, t.FeedId } equals new { st.TripId, st.FeedId }
                join r in _context.Routes on new { t.RouteId, t.FeedId } equals new { r.RouteId, r.FeedId }
                join s in _context.ServiceTypes on t.ServiceId equals s.ServiceId
                join stop in _context.Stops on new { st.StopId, st.FeedId } equals new { stop.StopId, stop.FeedId }
                where st.StopId == stopId && st.FeedId == feedId
                select new
                {
                    st.StopId,
                    stop.StopName,
                    DayType = s.DayType,
                    FeedId = r.FeedId,
                    r.RouteShortName,
                    st.DepartureTime,
                    t.TripId,
                    t.ServiceId
                };
        }
        else
        {
            var serviceIds = dateMappings.Select(x => x.ServiceId!).Distinct().ToList();

            query =
                from st in _context.StopTimes
                join t in _context.Trips on new { st.TripId, st.FeedId } equals new { t.TripId, t.FeedId }
                join r in _context.Routes on new { t.RouteId, t.FeedId } equals new { r.RouteId, r.FeedId }
                join stop in _context.Stops on new { st.StopId, st.FeedId } equals new { stop.StopId, stop.FeedId }
                where st.FeedId == feedId
                      && st.StopId == stopId
                      && serviceIds.Contains(t.ServiceId)
                      && st.StopSequence < _context.StopTimes
                          .Where(x => x.TripId == st.TripId && x.FeedId == st.FeedId)
                          .Max(x => x.StopSequence)
                select new
                {
                    st.StopId,
                    stop.StopName,
                    r.RouteShortName,
                    r.FeedId,
                    st.DepartureTime,
                    t.TripId,
                    t.ServiceId,
                    DayType = ""
                };
        }

        var rawData = await query.ToListAsync();

        if (!rawData.Any())
            return NotFound("Brak rozkładu dla wskazanego przystanku.");

        var serviceToDayType = dateMappings
            .GroupBy(x => x.ServiceId)
            .ToDictionary(g => g.Key!, g => g.First().DayType);

        var enriched = rawData.Select(x => new
        {
            x.StopId,
            x.StopName,
            x.RouteShortName,
            x.FeedId,
            x.DepartureTime,
            x.TripId,
            DayType = serviceToDayType.GetValueOrDefault(
            (string)(x.ServiceId ?? ""), 
            (string)(x.DayType ?? "Nieznany"))
        }).ToList();

        var result = new StopScheduleDto
        {
            StopId = stopId,
            StopName = enriched.First().StopName,
            Lines = enriched
                .GroupBy(x => x.RouteShortName)
                .OrderBy(g => g.Key)
                .Select(lineGroup => new RouteScheduleDto
                {
                    Route = lineGroup.Key,
                    FeedId = lineGroup.First().FeedId,
                    Days = lineGroup
                        .GroupBy(x => x.DayType)
                        .ToDictionary(
                            g => g.Key,
                            g => g.OrderBy(x => x.DepartureTime)
                                .Select(dep => new StopDepartureDto
                                {
                                    TripId = dep.TripId,
                                    FeedId = dep.FeedId,
                                    Time = dep.DepartureTime?.ToString(@"hh\:mm") ?? "??:??"
                                })
                                .ToList()
                        )
                })
                .ToList()
        };

        return Ok(result);
    }
}
