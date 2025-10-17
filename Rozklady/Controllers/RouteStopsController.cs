using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Dapper;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace Rozklady.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RouteStopsController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly ILogger<RouteStopsController> _logger;
    public RouteStopsController(IConfiguration config, ILogger<RouteStopsController> logger)
    {
        _config = config;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IEnumerable<RouteStopsDto>> GetRoutesStops([FromQuery] string feedId, [FromQuery] string routeId)
    {
         _logger.LogInformation("Getting route stops for feedId={FeedId} and routeId={RouteId}", feedId, routeId);
        using var conn = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));
        var sql = @"WITH trip_variants AS (
                    SELECT
                        t.feed_id,
                        t.route_id,
                        r.route_short_name,
                        t.direction_id,
                        st.trip_id,
                        string_agg(st.stop_id::text, '>' ORDER BY st.stop_sequence) AS stop_pattern
                    FROM trips t
                    JOIN routes r 
                        ON r.route_id = t.route_id AND r.feed_id = t.feed_id
                    JOIN stop_times st 
                        ON st.trip_id = t.trip_id
                        AND st.feed_id = t.feed_id  
                    WHERE t.feed_id = @feedId
                    AND t.route_id = @routeId
                    GROUP BY t.feed_id, t.route_id, r.route_short_name, t.direction_id, st.trip_id
                ),
                unique_variants AS (
                    SELECT 
                        feed_id AS FeedId,
                        route_id AS RouteId,
                        route_short_name,                  
                        direction_id AS DirectionId,
                        md5(stop_pattern) AS VariantId,
                        stop_pattern,
                        MIN(trip_id) AS sample_trip_id  
                    FROM trip_variants
                    GROUP BY feed_id, route_id, route_short_name, direction_id, stop_pattern   
                )
                SELECT
                    uv.VariantId,
                    uv.FeedId,
                    uv.RouteID,
                    uv.DirectionId,
                    uv.route_short_name AS RouteShortName,  
                    json_agg(
                        json_build_object(
                            'stop_sequence', st.stop_sequence,
                            'stop_id', st.stop_id,
                            'stop_name', s.stop_name,
                            'lat', s.stop_lat,
                            'lon', s.stop_lon
                        ) ORDER BY st.stop_sequence
                    ) AS Stops
                FROM unique_variants uv
                JOIN stop_times st 
                    ON st.trip_id = uv.sample_trip_id
                    AND st.feed_id = uv.FeedId
                JOIN stops s 
                    ON s.stop_id = st.stop_id
                    AND s.feed_id = uv.FeedId
                GROUP BY uv.VariantId, uv.FeedId, uv.RouteID, uv.DirectionId, uv.route_short_name
                ORDER BY uv.DirectionId, uv.VariantId;

            ";
        _logger.LogInformation("Query params: feedId={FeedId}, routeId={RouteId}", feedId, routeId);

        var result = await conn.QueryAsync<RouteStopsDto>(sql, new { feedId, routeId });
        _logger.LogInformation("Executing SQL: {Sql} with params feedId={FeedId}, routeId={RouteId}", sql, feedId, routeId);



    return result;
    }
}

