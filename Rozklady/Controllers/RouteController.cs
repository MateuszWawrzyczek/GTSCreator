using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Dapper;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rozklady.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoutesController : ControllerBase
{
    private readonly IConfiguration _config;

    public RoutesController(IConfiguration config)
    {
        _config = config;
    }

   [HttpGet]
    public async Task<IEnumerable<RouteDto>> GetRoutes()
    {
        using var conn = new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));
        var sql = @"SELECT 
                        feed_id AS FeedId, 
                        route_short_name AS RouteShortName,
                        route_id AS RouteId
                    FROM routes
                    ORDER BY FeedId, RouteShortName;";
        return await conn.QueryAsync<RouteDto>(sql);
    }
}