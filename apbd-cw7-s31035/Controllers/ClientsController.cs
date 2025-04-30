using apbd_cw7_s31035.Exceptions;
using apbd_cw7_s31035.Models;
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
        catch (Exception err) // Normally this would be handled in the middleware
        {
            Console.Error.WriteLine(err);
            return StatusCode(500, "Unexpected server error");
        }
    }

    [HttpPost]
    public async Task<IActionResult> PostClient(
        [FromBody] ClientCreateDTO client
    )
    {
        try
        {
            var clientId = await dbService.CreateClientAsync(client);
            return Ok(clientId);
        } 
        catch (Exception err) // Normally this would be handled in the middleware
        {
            Console.Error.WriteLine(err);
            return StatusCode(500, "Unexpected server error");
        }
    }

    [HttpPut]
    [Route("{clientId}/trips/{tripId}")]
    public async Task<IActionResult> RegisterClientOnTrip(
        [FromRoute] int clientId,
        [FromRoute] int tripId
    )
    {
        try
        {
            await dbService.RegisterClientOnTripAsync(clientId, tripId);
            return Ok("Registered Successfully!");
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        } 
        catch (Exception err) // Normally this would be handled in the middleware
        {
            Console.Error.WriteLine(err);
            return StatusCode(500, "Unexpected server error");
        }
    }

    [HttpDelete]
    [Route("{clientId}/trips/{tripId}")]
    public async Task<IActionResult> DeleteClientTrip(
        [FromRoute] int clientId,
        [FromRoute] int tripId
    )
    {
        try
        {
            await dbService.DeleteClientTrip(clientId, tripId);
            return Ok("Deleted Successfully");
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        } 
        catch (Exception err) // Normally this would be handled in the middleware
        {
            Console.Error.WriteLine(err);
            return StatusCode(500, "Unexpected server error");
        }
    }
}