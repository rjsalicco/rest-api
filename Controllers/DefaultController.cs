using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

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

    [HttpGet]
    [HttpPost]
    [HttpOptions]
    [Route("validate")]
    public ActionResult<string> OnValidate()
    {
        _logger.LogInformation("OnValidate invoked....");
        Guid myGuid = Guid.NewGuid();   
        Response.Headers.Add("WebHook-Allowed-Origin", "*");
        return myGuid.ToString();
    }

    [HttpGet]
    [HttpPost]
    [HttpOptions]
    [Route("connect")]
    public ActionResult<string> OnConnect()
    {
        _logger.LogInformation("OnConnect invoked....");
        Guid myGuid = Guid.NewGuid();
        Response.Headers.Add("WebHook-Allowed-Origin", "*");
        return myGuid.ToString();
    }


    [HttpPost]
    [HttpOptions]
    [Route("")]
    public async Task<ActionResult<string>> Any()
    {
        _logger.LogInformation("Any invoked....");

        Request.EnableBuffering();
        Request.Body.Position = 0;
        var userId = await new StreamReader(Request.Body).ReadToEndAsync();

        var audience = AudienceBuilder(userId);
        var token = GenerateToken(audience);

        using (var client = new HttpClient())
        {
            _logger.LogInformation("starting request");
            var url = audience + "?api-version=2022-22-01";
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
            var httpContent = new StringContent("It works!");
            var response = await client.PostAsync(url, httpContent);
            _logger.LogInformation("response: " + response);
        }
        
        Response.Headers.Add("WebHook-Allowed-Origin", "*");
        return "success!";
    }

    private string AudienceBuilder (string deviceName)
    {
        string endpointPrefix = "https://rj-demo.webpubsub.azure.com/api/hubs/default/users/";
        string endpointSuffix = "/:send";
        return endpointPrefix + deviceName + endpointSuffix;
    }

    private string GenerateToken(string audience)
    {
        var mySecret = "ooEojJdonmnywyD/HsbyR4mgaP0JhVNpmzhle9F0F3U=";
        var mySecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(mySecret));

        var tokenHandler = new JsonWebTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Expires = DateTime.UtcNow.AddDays(7),
            Audience = audience,
            SigningCredentials = new SigningCredentials(mySecurityKey, SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return token;
    }
}