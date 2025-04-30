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
        try
        {
            var trips = await dbService.GetAllTripsAsync();
            return Ok(trips);
        }
        catch (Exception err) // Normally this would be handled in the middleware
        {
            Console.Error.WriteLine(err);
            return StatusCode(500, "Unexpected server error");
        }
    }
}