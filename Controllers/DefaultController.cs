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
    [HttpOptions]
    [Route("connect")]
    public async Task<ActionResult<string>> Connect()
    {
        Guid myGuid = Guid.NewGuid();
        Response.Headers.Add("WebHook-Allowed-Origin", "*");
        return myGuid.ToString();
    }
       

    [HttpPost]
    [HttpOptions]
    [Route("")]
    public async Task<ActionResult<string>> Any()
    {
        var audience = AudienceBuilder("device1");
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
        string endpointPrefix = "https://default-pubsub.webpubsub.azure.com/api/hubs/hub1/users/";
        string endpointSuffix = "/:send";
        return endpointPrefix + deviceName + endpointSuffix;
    }

    private string GenerateToken(string audience)
    {
        var mySecret = "IsRVALjDuxlFe1OJ2twrv/8Coeos1HQgaYvMlaE9q5U=";
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