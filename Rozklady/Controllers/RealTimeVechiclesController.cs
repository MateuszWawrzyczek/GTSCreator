using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class VehiclesController : ControllerBase
{
    [HttpGet("vehiclePositions")]
    public IActionResult GetVehiclePositions()
    {
        return Ok(RealTimeVehiclesService.GetCache());
    }
}
