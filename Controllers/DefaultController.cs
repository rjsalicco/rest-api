using Microsoft.AspNetCore.Mvc;

namespace Controllers;

[ApiController]
[Route("/api")]
public class DefaultController : ControllerBase
{
    private readonly ILogger<DefaultController> _logger;

    public DefaultController(ILogger<DefaultController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    [Route("/connect")]
    public string Connect()
    {
        return "Connected!";
    }

    [HttpPost]
    [Route("/message")]
    public string Message()
    {
        return "Message received!";
    }

    [HttpPost]
    [Route("/disconnect")]
    public string Disconnect()
    {
        return "Disconnected!";
    }

    [HttpPost]
    [Route("/")]
    public string Any()
    {
        return "Working!";
    }
}