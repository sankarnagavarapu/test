using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace MyWebApi1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet("GetWeatherData")]
        public IActionResult GetWeatherData(string state)
        {
            var token = HttpContext.Session.GetString("AccessToken");

            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("No token Authenticated.");
            }

            return Ok(new { Message = "Data fetched successfully", Token = token });
        }

        //[Authorize(Policy = "ReadScope")]
        [HttpGet("read")]
        [Authorize(Roles = "ReadAccess")]

        public IActionResult ReadData()
        {   
            return Ok(new { Message = "You have read access." });
        }


        /*[HttpGet("read")]
        [Authorize]
        public IActionResult ReadData()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            return Ok(new { Message = "You have read access.", Claims = claims });
        }*/

        //[Authorize(Policy = "ReadWriteScope")]
        [HttpPost("write")]
        [Authorize(Roles = "WriteAccess")]
        public IActionResult WriteData()
        {
            return Ok(new { Message = "You have write access." });
        }

        /*[HttpGet]
         [Authorize]
         public IActionResult Get()
         {
             return Ok(new { Message = "Authenticated response from Azure AD protected API" });
         }*/
        [HttpGet]
        [Authorize]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
