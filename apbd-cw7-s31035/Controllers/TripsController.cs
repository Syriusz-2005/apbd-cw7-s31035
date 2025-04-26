using apbd_cw7_s31035.Services;
using Microsoft.AspNetCore.Mvc;

namespace apbd_cw7_s31035.Controllers;

[ApiController]
[Route("/api/trips")]
public class TripsController(IDbService dbService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllTrips()
    {
        var trips = await dbService.GetAllTripsAsync();
        return Ok(trips);
    }
}