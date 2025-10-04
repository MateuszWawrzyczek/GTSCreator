using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/gtfs")]
public class GtfsController : ControllerBase
{
    private readonly GtfsFacade _facade;

    public GtfsController(GtfsFacade facade)
    {
        _facade = facade;
    }

    [HttpGet("{prefix}")]
    public async Task<IActionResult> GetGtfs(string prefix)
    {
        var customer = new Customer { Prefix = prefix, Name = prefix, Domain = "kiedyprzyjedzie.pl" };
        var zipBytes = await _facade.GenerateGtfsForProviderAsync(customer);

        if (zipBytes == null || zipBytes.Length == 0)
            return NotFound("Nie znaleziono danych GTFS dla podanego prefiksu.");

        return File(zipBytes, "application/zip", $"{prefix}.zip");
    }
}
