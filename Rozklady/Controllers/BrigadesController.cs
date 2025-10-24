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
                .OrderBy(t => ParseBlockId(t.BlockId ?? "").prefix)   // alfabetycznie po prefixie
                .ThenBy(t => ParseBlockId(t.BlockId ?? "").number)   // numerycznie po numerze
                .ThenBy(t => t.StartTime)                            // potem po czasie
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

        var cleaned = Regex.Replace(blockId, @"(RB|RF|SB|ND|SW)$", "", RegexOptions.IgnoreCase);

        var matchDash = Regex.Match(cleaned, @"^(?<prefix>[A-ZŁĄĆĘŚŻŹÖÜĆ]+)-?(?<num>\d+)$", RegexOptions.IgnoreCase);
        if (matchDash.Success)
        {
            string prefix = matchDash.Groups["prefix"].Value ?? "";
            int number = int.TryParse(matchDash.Groups["num"].Value, out int n) ? n : int.MaxValue;
            return (prefix.ToUpperInvariant(), number);
        }

        var matchPlain = Regex.Match(cleaned, @"^(?<prefix>[A-ZŁĄĆĘŚŻŹÖÜĆ]*)(?<num>\d+)$", RegexOptions.IgnoreCase);
        if (matchPlain.Success)
        {
            string prefix = matchPlain.Groups["prefix"].Value ?? "";
            int number = int.TryParse(matchPlain.Groups["num"].Value, out int n) ? n : int.MaxValue;
            return (prefix.ToUpperInvariant(), number);
        }

        return ("", int.MaxValue);
    }


}
