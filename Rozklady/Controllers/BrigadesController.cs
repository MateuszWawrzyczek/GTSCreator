using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rozklady.Data;
using Rozklady.Models;
using System.Text.RegularExpressions;

[ApiController]
[Route("api/[controller]")]
public class BlocksController : ControllerBase
{
    private readonly IDbContextFactory<RozkladyContext> _contextFactory;

    public BlocksController(IDbContextFactory<RozkladyContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    [HttpGet]
    public async Task<IActionResult> GetBlocks()
    {
        await using var db = _contextFactory.CreateDbContext();

        var trips = await db.Trips
            .Where(t => t.BlockId != null && (t.FeedId == "MZK" || t.FeedId == "KMR"))
            .Select(t => new { t.TripId, t.BlockId, t.FeedId, t.RouteId, t.TripHeadsign })
            .ToListAsync();

        var tripIds = trips.Select(t => t.TripId).ToList();

        var stopTimes = await db.StopTimes
            .Where(st => tripIds.Contains(st.TripId))
            .GroupBy(st => st.TripId)
            .Select(g => new
            {
                TripId = g.Key,
                StartTime = g.Min(st => st.DepartureTime),
                EndTime = g.Max(st => st.ArrivalTime)
            })
            .ToListAsync();

        var detailedTrips = trips
            .Select(t => new
            {
                t.FeedId,
                t.BlockId,
                t.RouteId,
                t.TripHeadsign,
                StartTime = stopTimes.FirstOrDefault(st => st.TripId == t.TripId)?.StartTime,
                EndTime = stopTimes.FirstOrDefault(st => st.TripId == t.TripId)?.EndTime
            })
            .ToList();

            detailedTrips = detailedTrips
                .OrderBy(t => ParseBlockId(t.BlockId ?? "").prefix == "" ? 0 : 1) 
                .ThenBy(t => ParseBlockId(t.BlockId ?? "").number)
                .ThenBy(t => ParseBlockId(t.BlockId ?? "").prefix)
                .ThenBy(t => t.StartTime)
                .ToList();



        var groupedBlocks = detailedTrips
            .GroupBy(t => t.BlockId)
            .Select(g => new
            {
                BlockId = g.Key,
                Trips = g.ToList()
            })
            .ToList();

        return Ok(groupedBlocks);
    }

    private static (string prefix, int number) ParseBlockId(string blockId)
    {
        if (string.IsNullOrWhiteSpace(blockId))
            return ("", int.MaxValue);

        var cleaned = Regex.Replace(blockId, @"(RB|RF|SB|ND|SW)$", "");

        var match = Regex.Match(cleaned, @"^(?<prefix>[A-Z])?-?(?<num>\d+)");
        if (!match.Success)
            return ("", int.MaxValue);

        string prefix = match.Groups["prefix"].Value ?? "";
        int number = int.TryParse(match.Groups["num"].Value, out int n) ? n : int.MaxValue;

        return (prefix, number);
    }

}
