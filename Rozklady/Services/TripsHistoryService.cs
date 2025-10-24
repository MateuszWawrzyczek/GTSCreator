using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using Rozklady.Data;
using Rozklady.Models;

public class TripsHistoryService
{
    private readonly IDbContextFactory<RozkladyContext> _contextFactory;
    private readonly ConcurrentDictionary<string, ActiveTripDto> _activeTrips = new();
    private readonly PrefixService _prefixService;

    private readonly TimeZoneInfo _warsawTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");

    public TripsHistoryService(IDbContextFactory<RozkladyContext> contextFactory, PrefixService prefixService)
    {
        _contextFactory = contextFactory;
        _prefixService = prefixService;
    }

    public async Task ProcessVehiclePositions(IEnumerable<VehicleDto> vehicles)
    {
        foreach (var vehicle in vehicles)
        {
            await ProcessVehiclePosition(vehicle);
        }
    }

    public async Task ProcessVehiclePosition(VehicleDto pos)
    {
        var key = $"{pos.FleetNumber}:{pos.FeedId}";
        DateTime nowLocal = DateTime.SpecifyKind(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _warsawTimeZone), DateTimeKind.Unspecified);

        int? prefix = pos.FeedId == "MZK" ? _prefixService.MzkPrefix : _prefixService.KmrPrefix;
        string prefixedTripId = $"{prefix}_{pos.TripId}";

        if (_activeTrips.TryGetValue(key, out var existing))
        {
            if (existing.TripId != pos.TripId)
            {
                await EndTripAsync(existing, nowLocal);
                _activeTrips.TryRemove(key, out _);
            }

            if (pos.OnTrip)
            {
                existing.LastSeen = nowLocal;
                existing.Delay = ParseDelay(pos.Delay);
                existing.LastLat = pos.Latitude;
                existing.LastLon = pos.Longitude;
            }
            else
            {
                await EndTripAsync(existing, nowLocal);
                _activeTrips.TryRemove(key, out _);
            }
        }
        else
        {
            if (!pos.OnTrip) return;

            var trip = new ActiveTripDto
            {
                FeedId = pos.FeedId!,
                RouteId = pos.RouteId!,
                TripId = pos.TripId!,
                FleetNumber = pos.FleetNumber!,
                FirstSeen = nowLocal,
                LastSeen = nowLocal,
                Delay = ParseDelay(pos.Delay),
                LastLat = pos.Latitude,
                LastLon = pos.Longitude
            };

            _activeTrips[key] = trip;

            await using var db = _contextFactory.CreateDbContext();
            DateTime today = DateTime.SpecifyKind(nowLocal.Date, DateTimeKind.Unspecified);

            var stops = await db.StopTimes
                .Where(s => s.FeedId == trip.FeedId && s.TripId == prefixedTripId)
                .OrderBy(s => s.StopSequence)
                .Select(s => new { s.DepartureTime, s.ArrivalTime })
                .ToListAsync();

            var firstStop = stops?.FirstOrDefault();
            var lastStop = stops?.LastOrDefault();

            DateTime? plannedStart = firstStop?.DepartureTime != null
                ? DateTime.SpecifyKind(today + firstStop.DepartureTime.Value, DateTimeKind.Unspecified)
                : (DateTime?)null;

            DateTime? plannedEnd = lastStop?.ArrivalTime != null
                ? DateTime.SpecifyKind(today + lastStop.ArrivalTime.Value, DateTimeKind.Unspecified)
                : (DateTime?)null;

            var tripInfo = await db.Trips
                .Where(t => t.FeedId == trip.FeedId && t.TripId == prefixedTripId)
                .Select(t => new { t.TripHeadsign })
                .FirstOrDefaultAsync();

            string direction = tripInfo?.TripHeadsign ?? "Unknown";

            await db.TripsHistory.AddAsync(new TripsHistory
            {
                FeedId = trip.FeedId!,
                RouteId = trip.RouteId!,
                TripId = prefixedTripId,
                FleetNumber = trip.FleetNumber!,
                ActualStartTime = nowLocal,
                Direction = direction!,
                PlannedStartTime = plannedStart,
                PlannedEndTime = plannedEnd
            });

            await db.SaveChangesAsync();
        }
    }

    private async Task EndTripAsync(ActiveTripDto trip, DateTime nowLocal)
    {
        int? prefix = trip.FeedId == "MZK" ? _prefixService.MzkPrefix : _prefixService.KmrPrefix;
        string prefixedTripId = $"{prefix}_{trip.TripId}";

        await using var db = _contextFactory.CreateDbContext();

        var history = await db.TripsHistory
            .FirstOrDefaultAsync(h =>
                h.TripId == prefixedTripId &&
                h.FleetNumber == trip.FleetNumber &&
                h.ActualEndTime == null);

        if (history != null)
        {
            history.ActualEndTime = nowLocal;
            await db.SaveChangesAsync();
        }
    }

    public async Task CheckInactiveTripsAsync()
    {
        DateTime nowLocal = DateTime.SpecifyKind(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _warsawTimeZone), DateTimeKind.Unspecified);
        var timeout = TimeSpan.FromMinutes(0.5);

        foreach (var (key, trip) in _activeTrips)
        {
            if (nowLocal - trip.LastSeen > timeout)
            {
                _activeTrips.TryRemove(key, out _);

                int? prefix = trip.FeedId == "MZK" ? _prefixService.MzkPrefix : _prefixService.KmrPrefix;
                string prefixedTripId = $"{prefix}_{trip.TripId}";

                await using var db = _contextFactory.CreateDbContext();
                var history = await db.TripsHistory
                    .FirstOrDefaultAsync(h =>
                        h.TripId == prefixedTripId &&
                        h.FleetNumber == trip.FleetNumber &&
                        h.ActualEndTime == null);

                if (history != null)
                {
                    history.ActualEndTime = trip.LastSeen;
                    await db.SaveChangesAsync();
                }
            }
        }
    }

    private TimeSpan? ParseDelay(string? delayString)
    {
        if (string.IsNullOrEmpty(delayString))
            return null;
        if (TimeSpan.TryParse(delayString, out var delay))
            return delay;
        return null;
    }
}
