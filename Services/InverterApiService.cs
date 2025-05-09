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

public class InverterApiService
{
    private readonly IHttpClientFactory _factory;
    private readonly IConfiguration _configuration;
    private readonly string baseUrl;
    private readonly string email;
    private readonly string password;
    public InverterApiService(IHttpClientFactory factory, IConfiguration configuration)
    {
        _factory = factory;
        _configuration = configuration;
        baseUrl = _configuration.GetValue<string>("InverterDataApi:BaseUrl");
        email = _configuration.GetValue<string>("InverterDataApi:Email");
        password = _configuration.GetValue<string>("InverterDataApi:Password");
    }

    public async Task<InverterApiAuthResponse> GetToken()
    {
        var client = _factory.CreateClient();

        client.BaseAddress = new Uri(baseUrl);

        InverterApiAuthRequest inverterApiAuthModel = new InverterApiAuthRequest
        {
            email = email,
            password = password
        };
        var company = JsonConvert.SerializeObject(inverterApiAuthModel);
        var requestContent = new StringContent(company, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("Auth/login", requestContent);

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var inverterApiAuthResponse = JsonConvert.DeserializeObject<InverterApiAuthResponse>(content);

            return inverterApiAuthResponse;
        }
        else
        {
            throw new HttpRequestException($"Failed to get token. Status code: {response.StatusCode}");
        }
    }

    public async Task RefreshInverterData(string token)
    {
        var client = _factory.CreateClient();

        client.BaseAddress = new Uri(baseUrl);

        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        InverterApiAuthRequest inverterApiAuthModel = new InverterApiAuthRequest
        {
            email = email,
            password = password
        };
        var company = JsonConvert.SerializeObject(inverterApiAuthModel);
        var requestContent = new StringContent(company, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("Refresh/RefreshInverterData", null);
    }
}
