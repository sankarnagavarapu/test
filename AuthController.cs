using Microsoft.AspNetCore.Mvc;
using System;
using System.Text.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Headers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;

    private static Dictionary<string, string> TokenStore = new();

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet("login")]
    public IActionResult Login()
    {
        var authorizationEndpoint = $"{_configuration["AzureAd:Instance"]}{_configuration["AzureAd:TenantId"]}/oauth2/v2.0/authorize";
        var clientId = _configuration["AzureAd:ClientId"];
        var redirectUri = _configuration["AzureAd:RedirectUri"];
        var scope = _configuration["AzureAd:Scopes"];
        var responseType = "code";

        var query = $"client_id={clientId}&response_type={responseType}&redirect_uri={redirectUri}&response_mode=query&scope={scope}&state=12345";
        var loginUrl = $"{authorizationEndpoint}?{query}";

        return Redirect(loginUrl);
    }

    [HttpGet("redirect")]
    public async Task<IActionResult> RedirectBack(string code, string state)
    {
        var tokenEndpoint = $"{_configuration["AzureAd:Instance"]}{_configuration["AzureAd:TenantId"]}/oauth2/v2.0/token";

        var body = new FormUrlEncodedContent(new[]
        {
        new KeyValuePair<string, string>("client_id", _configuration["AzureAd:ClientId"]),
        new KeyValuePair<string, string>("scope", "https://graph.microsoft.com/.default"),
        new KeyValuePair<string, string>("code", code),
        new KeyValuePair<string, string>("redirect_uri", _configuration["AzureAd:RedirectUri"]),
        new KeyValuePair<string, string>("grant_type", "authorization_code"),
        new KeyValuePair<string, string>("client_secret", _configuration["AzureAd:ClientSecret"])
    });

        using var httpClient = new HttpClient();
        var response = await httpClient.PostAsync(tokenEndpoint, body);

        if (response.IsSuccessStatusCode)
        {
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<JsonElement>(jsonResponse);
            var accessToken = tokenResponse.GetProperty("access_token").GetString();
            return Redirect("LoginSuccess");

            HttpContext.Session.SetString("AccessToken", accessToken);
            //TokenStore[state] = accessToken;

            // Call the GetWeatherData API with the token
            /*var apiUrl = $"http://localhost:5117/api/weatherForecast/GetWeatherData?state={state}";
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var weatherResponse = await httpClient.GetAsync(apiUrl);


            if (weatherResponse.IsSuccessStatusCode)
            {
                var weatherData = await weatherResponse.Content.ReadAsStringAsync();
                return Ok(new { Message = "Authenticated and fetched weather data successfully", WeatherData = weatherData });
            }

            return BadRequest("Failed to fetch weather data.");
        }*/


        }
        return BadRequest("Failed to exchange authorization code for token.");
    }

    [HttpGet("LoginSuccess")]
    public IActionResult LoginSuccess()
    {
        return Ok("Login successful. You can now access protected resources.");
    }
    /*public static string GetToken(string state)
    {
        if (TokenStore.TryGetValue(state, out var token))
            {
            
                return token;
            }
            throw new KeyNotFoundException();
        //return TokenStore.TryGetValue(state, out var token) ? token : null;
    }*/
}
