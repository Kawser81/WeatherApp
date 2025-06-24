using Microsoft.AspNetCore.Mvc;
using WeatherApp.Models;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace WeatherApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey = "6a81701661c017710f417b71be20db45"; // Your OpenWeatherMap API key

        public HomeController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        public IActionResult Index()
        {
            return View(new WeatherData
            {
                City = string.Empty,
                Description = string.Empty,
                Icon = string.Empty,
                WindDescription = string.Empty
            });
        }

        [HttpPost]
        public async Task<IActionResult> Index(string city)
        {
            if (string.IsNullOrEmpty(city))
            {
                ViewBag.Error = "Please enter a city name.";
                return View(new WeatherData
                {
                    City = string.Empty,
                    Description = string.Empty,
                    Icon = string.Empty,
                    WindDescription = string.Empty
                });
            }

            try
            {
                var weatherUrl = $"https://api.openweathermap.org/data/2.5/weather?q={city}&units=metric&appid={_apiKey}";
                var weatherResponse = await _httpClient.GetStringAsync(weatherUrl);
                var weatherData = JsonSerializer.Deserialize<JsonElement>(weatherResponse);

                var windDegrees = weatherData.GetProperty("wind").GetProperty("deg").GetDouble();
                var windSpeed = weatherData.GetProperty("wind").GetProperty("speed").GetDouble();

                var weather = new WeatherData
                {
                    City = city,
                    Temperature = weatherData.GetProperty("main").GetProperty("temp").GetDouble(),
                    FeelsLike = weatherData.GetProperty("main").GetProperty("feels_like").GetDouble(),
                    Description = weatherData.GetProperty("weather")[0].GetProperty("description").GetString() ?? "N/A",
                    Icon = weatherData.GetProperty("weather")[0].GetProperty("icon").GetString() ?? "01d",
                    Humidity = (int)weatherData.GetProperty("main").GetProperty("humidity").GetDouble(), // fixed
                    WindSpeed = windSpeed,
                    WindDirection = GetWindDirection(windDegrees),
                    WindDescription = GetWindDescription(windSpeed),
                    Pressure = weatherData.GetProperty("main").GetProperty("pressure").GetDouble(),
                    UVIndex = 0.0, // not available in current API
                    DewPoint = 0.0, // not available in current API
                    Visibility = (int)(weatherData.GetProperty("visibility").GetDouble() / 1000) // fixed
                };

                return View(weather);
            }
            catch (HttpRequestException ex)
            {
                ViewBag.Error = $"API request failed: {ex.Message}. Please check your API key or internet connection.";
                return View(new WeatherData
                {
                    City = string.Empty,
                    Description = string.Empty,
                    Icon = string.Empty,
                    WindDescription = string.Empty
                });
            }
            catch (JsonException ex)
            {
                ViewBag.Error = $"Error parsing weather data: {ex.Message}";
                return View(new WeatherData
                {
                    City = string.Empty,
                    Description = string.Empty,
                    Icon = string.Empty,
                    WindDescription = string.Empty
                });
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Unexpected error: {ex.Message}. StackTrace: {ex.StackTrace}";
                return View(new WeatherData
                {
                    City = string.Empty,
                    Description = string.Empty,
                    Icon = string.Empty,
                    WindDescription = string.Empty
                });
            }
        }

        private string GetWindDirection(double degrees)
        {
            string[] directions = {
                "N", "NNE", "NE", "ENE", "E", "ESE", "SE", "SSE",
                "S", "SSW", "SW", "WSW", "W", "WNW", "NW", "NNW"
            };
            int index = (int)((degrees + 11.25) / 22.5) % 16;
            return directions[index];
        }

        private string GetWindDescription(double speed)
        {
            return speed switch
            {
                < 0.3 => "Calm",
                < 1.6 => "Light air",
                < 3.4 => "Light breeze",
                < 5.5 => "Gentle breeze",
                < 8.0 => "Moderate breeze",
                < 10.8 => "Fresh breeze",
                < 13.9 => "Strong breeze",
                _ => "Gale"
            };
        }
    }
}
