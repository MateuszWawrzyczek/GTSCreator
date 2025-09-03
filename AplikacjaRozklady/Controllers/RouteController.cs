using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Dapper;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        var sql = @"SELECT route_id, route_short_name, feed_id
                    FROM routes 
                    ORDER BY route_short_name";
        return await conn.QueryAsync<RouteDto>(sql);
    }
}
