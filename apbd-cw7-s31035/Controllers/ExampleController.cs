using apbd_cw7_s31035.Services;
using Microsoft.AspNetCore.Mvc;

namespace apbd_cw7_s31035.Controllers;

[ApiController]
[Route("/whatever")]
public class ExampleController(IDbService dbService) : ControllerBase
{

}