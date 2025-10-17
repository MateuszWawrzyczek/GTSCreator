using Microsoft.AspNetCore.Mvc;
using Rozklady.Data;
using Rozklady.Models;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class TimetableController : ControllerBase
{
    private readonly RozkladyContext _context;

    public TimetableController(RozkladyContext context)
    {
        _context = context;
    }
    [HttpGet]
    public async Task<List<ServiceDto>> GetActiveServiceIds(DateOnly date)
    {
        var exceptions = await _context.CalendarDates
            .Where(cd => cd.Date == date)
            .ToListAsync();

        var active = new HashSet<(string, string)>();

        foreach (var e in exceptions.Where(cd => cd.ExceptionType == 1))
            active.Add((e.FeedId, e.ServiceId));

        var removed = exceptions
            .Where(cd => cd.ExceptionType == 2)
            .Select(cd => (cd.FeedId, cd.ServiceId))
            .ToHashSet();

        var weekday = date.DayOfWeek switch
        {
            DayOfWeek.Monday => nameof(Calendar.Monday),
            DayOfWeek.Tuesday => nameof(Calendar.Tuesday),
            DayOfWeek.Wednesday => nameof(Calendar.Wednesday),
            DayOfWeek.Thursday => nameof(Calendar.Thursday),
            DayOfWeek.Friday => nameof(Calendar.Friday),
            DayOfWeek.Saturday => nameof(Calendar.Saturday),
            DayOfWeek.Sunday => nameof(Calendar.Sunday),
            _ => throw new Exception("Nieznany dzień tygodnia")
        };

        var fromCalendar = await _context.Calendars
            .Where(c =>
                c.StartDate <= date && c.EndDate >= date &&
                EF.Property<int>(c, weekday) == 1)
            .Select(c => new { c.FeedId, c.ServiceId })
            .ToListAsync();

        foreach (var s in fromCalendar)
        {
            if (!removed.Contains((s.FeedId, s.ServiceId)))
                active.Add((s.FeedId, s.ServiceId));
        }

        return active
            .Select(a => new ServiceDto { FeedId = a.Item1, ServiceId = a.Item2 })
            .ToList();
    }

    [HttpGet("departures")]
    public async Task<ActionResult<StopDeparturesDto>> GetDepartures(
        DateOnly date, string FeedId, string stopId, int hours = 5, int max = 20)
    {
        var activeServices = await GetActiveServiceIds(date);

        if (!activeServices.Any())
            return new StopDeparturesDto { StopName = "", Departures = new List<DepartureDto>() };

        var activeKeys = activeServices
            .Where(s => s.FeedId == FeedId)
            .Select(s => s.ServiceId)
            .ToList();

        if (!activeKeys.Any())
            return new StopDeparturesDto { StopName = "", Departures = new List<DepartureDto>() };

        var stopName = await _context.Stops
            .Where(s => s.FeedId == FeedId && s.StopId == stopId)
            .Select(s => s.StopName)
            .FirstOrDefaultAsync();

        if (stopName == null)
            return NotFound();

        var trips = await _context.Trips
            .Where(t => t.FeedId == FeedId && activeKeys.Contains(t.ServiceId))
            .Include(t => t.StopTimes)
            .Include(t => t.Route)
            .ToListAsync();

        var now = DateTime.Now.TimeOfDay;
        var maxTime = now.Add(TimeSpan.FromHours(hours));

        var vehicles = VehicleCache.GetCache(); 
        var vehicleLookup = vehicles
            .Where(v => v.FeedId == FeedId && v.TripId != null)
            .GroupBy(v => v.TripId!)
            .ToDictionary(g => g.Key!, g => g.First());
        
        string StripPrefix(string tripId)
        {
            var parts = tripId.Split('_');
            return parts.Length > 1 ? parts.Last() : tripId;
        }
        

        var departures = trips
        .SelectMany(t => t.StopTimes, (trip, st) => new { trip, st })
        .Where(x => x.st.StopId == stopId)
        .Where(x => x.st.DepartureTime >= now && x.st.DepartureTime <= maxTime)
        .Select(x =>
        {
            var strippedTripId = StripPrefix(x.trip.TripId);
            var vehicle = vehicleLookup
                .Where(kv => x.trip.TripId.EndsWith("_" + kv.Key))
                .Select(kv => kv.Value)
                .FirstOrDefault();
            return new DepartureDto
            {
                FeedId = x.trip.FeedId,
                TripId = x.trip.TripId,
                StopId = x.st.StopId,
                Headsign = x.trip.TripHeadsign,
                RouteShortName = x.trip.Route?.RouteShortName ?? "",
                DepartureTime = x.st.DepartureTime ?? TimeSpan.Zero,
                Delay = vehicle?.Delay ?? "",
                FleetNumber = vehicle?.FleetNumber ?? "",
                OnTrip = vehicle?.OnTrip ?? false
            };
        })
        .OrderBy(d => d.DepartureTime)
        .Take(max)
        .ToList();

        return new StopDeparturesDto
        {
            StopName = stopName ?? "",
            Departures = departures
        };
    }
}





