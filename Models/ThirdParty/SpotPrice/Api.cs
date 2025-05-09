using Newtonsoft.Json;
using SmartElectricityAPI.ThirdParty.SpotHintaFi;

namespace SmartElectricityAPI.Models.ThirdParty.SpotHintaFi;

public class Api
{
    public static async Task<List<SpotPrice>> GetTodayPrices(string regionAbbrevation)
    {
        Uri siteUri = new Uri(Constants.BaseUrl);

        var client = new HttpClient();
        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json; charset=utf-8");
        client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Encoding", "gzip");

        var response = await client.GetAsync($"{siteUri.AbsoluteUri}Today?region={regionAbbrevation}");

        if (response.StatusCode == System.Net.HttpStatusCode.OK)
        {
            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<SpotPrice>>(responseJson)!;
        }
        else
        {
            return null;
        }        
    }

    public static async Task<List<SpotPrice>?> GetDayForwardPrices(string regionAbbrevation)
    {
        Uri siteUri = new Uri(Constants.BaseUrl);

        var client = new HttpClient();
        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json; charset=utf-8");
        client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Encoding", "gzip");

        var response = await client.GetAsync($"{siteUri.AbsoluteUri}DayForward?region={regionAbbrevation}");

        if (response.StatusCode == System.Net.HttpStatusCode.OK)
        {
            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<SpotPrice>>(responseJson)!;
        }
        else
        {
            Console.WriteLine($"Error response code on dayforward prices. Method: GetDayForwardPrices ");
            return null;
        }
    }
}
