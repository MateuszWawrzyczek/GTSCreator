using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using Rozklady.Data;
using Rozklady.Models;


public class TripsHistoryService
{
    private readonly RozkladyContext _context;
    private readonly ConcurrentDictionary<string, ActiveTripDto> _activeTrips = new();

    public TripsHistoryService(RozkladyContext context)
    {
        _context = context;

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
        var now = DateTime.UtcNow;

        // Sprawdź, czy pojazd ma aktywny kurs w pamięci
        if (_activeTrips.TryGetValue(key, out var existing))
        {
            // 1️⃣ Jeśli kurs się zmienił (inne TripId)
            if (existing.TripId != pos.TripId)
            {
                await EndTripAsync(existing, now);
                _activeTrips.TryRemove(key, out _);
            }

            // 2️⃣ Aktualizuj dane bieżącego kursu
            if (pos.OnTrip)
            {
                existing.LastSeen = now;
                existing.Delay = ParseDelay(pos.Delay);
                existing.LastLat = pos.Latitude;
                existing.LastLon = pos.Longitude;
            }
            else
            {
                // 3️⃣ Kurs się zakończył
                await EndTripAsync(existing, now);
                _activeTrips.TryRemove(key, out _);
            }
        }
        else
        {
            // 4️⃣ Nowy kurs (onTrip = true)
            if (pos.OnTrip)
            {
                var trip = new ActiveTripDto
                {
                    FeedId = pos.FeedId!,
                    RouteId = pos.RouteId!,
                    TripId = pos.TripId!,
                    FleetNumber = pos.FleetNumber!,
                    FirstSeen = now,
                    LastSeen = now,
                    Delay = ParseDelay(pos.Delay),
                    LastLat = pos.Latitude,
                    LastLon = pos.Longitude
                };

                _activeTrips[key] = trip;

                // Zapisz rozpoczęcie w bazie
                await _context.TripsHistory.AddAsync(new TripsHistory
                {
                    FeedId = trip.FeedId!,
                    RouteId = trip.RouteId!,
                    TripId = trip.TripId!,
                    FleetNumber = trip.FleetNumber!,
                    ActualStartTime = now
                });
                await _context.SaveChangesAsync();
            }
        }
    }

    // Pomocnicza metoda kończenia kursu
    private async Task EndTripAsync(ActiveTripDto trip, DateTime now)
    {
        var history = await _context.TripsHistory
            .FirstOrDefaultAsync(h =>
                h.TripId == trip.TripId &&
                h.FleetNumber == trip.FleetNumber &&
                h.ActualEndTime == null);

        if (history != null)
        {
            history.ActualEndTime = now;
            //history.DelayEndSeconds = (int?)trip.Delay?.TotalSeconds;
            await _context.SaveChangesAsync();
        }
    }


    public async Task CheckInactiveTripsAsync()
    {
        var now = DateTime.UtcNow;
        var timeout = TimeSpan.FromMinutes(0.5);

        foreach (var (key, trip) in _activeTrips)
        {
            if (now - trip.LastSeen > timeout)
            {
                _activeTrips.TryRemove(key, out _);

                var history = await _context.TripsHistory
                    .FirstOrDefaultAsync(h => h.TripId == trip.TripId &&
                                            h.FleetNumber == trip.FleetNumber &&
                                            h.ActualEndTime == null);

                if (history != null)
                {
                    history.ActualEndTime = trip.LastSeen;
                    //history.DelayEndSeconds = (int?)trip.Delay?.TotalSeconds;
                }
            }
        }

        await _context.SaveChangesAsync();

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