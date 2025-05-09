using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using SmartElectricityAPI.Models;
using System.Net.Http;
using System.Runtime;
using System.Text;

using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace SmartElectricityAPI.Services;

public class WeatherApiComService
{
    private readonly IHttpClientFactory _factory;
    private readonly IConfiguration _configuration;
    private readonly string baseUrl;
    private readonly string apiKey;
    public WeatherApiComService(IHttpClientFactory factory, IConfiguration configuration)
    {
        _factory = factory;
        _configuration = configuration;
        baseUrl = _configuration.GetValue<string>("WeatherApiCom:BaseUrl");
        apiKey = _configuration.GetValue<string>("WeatherApiCom:ApiKey");
    }

    public async Task<WeatherApiResponse> GetWeatherData(string location, DateTime date)
    {
      //  DateTime date = DateTime.Now.AddDays(1);

        var client = _factory.CreateClient();

        client.BaseAddress = new Uri(baseUrl);

        var response = await client.GetAsync($"forecast.json?q={location}&days=1&dt={date.ToString("yyyy-MM-dd")}&key={apiKey}");

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<WeatherApiResponse>(content);

        }
        else
        {
            throw new HttpRequestException($"Failed to get weather data. Status code: {response.StatusCode}");
        }
    }




}
