using apbd_cw7_s31035.Exceptions;
using apbd_cw7_s31035.Services;
using Microsoft.AspNetCore.Mvc;

namespace apbd_cw7_s31035.Controllers;

[ApiController]
[Route("/api/clients")]
public class ClientsController(IDbService dbService) : ControllerBase
{
    [HttpGet]
    [Route("{clientId}/trips")]
    public async Task<IActionResult> GetClientTrips(
        [FromRoute] int clientId
    )
    {
        try
        {
            var clientTrips = await dbService.GetClientTripsAsync(clientId);
            return Ok(clientTrips);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}