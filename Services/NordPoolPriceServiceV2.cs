using SmartElectricityAPI.Models.ThirdParty.SpotHintaFi;
using SmartElectricityAPI.Models;
using Newtonsoft.Json;
using System.Net;
using System.Xml.XPath;


namespace SmartElectricityAPI.Services;

public class NordPoolPriceServiceV2
{
    public async Task<List<SpotPrice>> GetPrices(string abbrevation, DateOnly dateOnly)
    {
        
        var nordpoolUrl = $"https://dataportal-api.nordpoolgroup.com/api/DayAheadPrices?date={dateOnly.ToString("yyyy-MM-dd")}&market=DayAhead&deliveryArea={abbrevation}&currency=EUR";

        var consumerPrices = new List <ConsumerPriceV2>();

        try
        {
            var request = WebRequest.Create(nordpoolUrl);
            request.Method = "GET";
            request.ContentType = "application/json";
            request.Headers.Add("Accept", "application/json");

            using (var response = await request.GetResponseAsync())
            {
                using (var stream = response.GetResponseStream())
                {
                    using (var reader = new StreamReader(stream))
                    {
                        var json = await reader.ReadToEndAsync();
                        var nordPoolPriceResponse = JsonConvert.DeserializeObject<NordPoolPriceResponse>(json);

                        if (nordPoolPriceResponse != null)
                        {
                            consumerPrices = nordPoolPriceResponse.MultiAreaEntries.Select(entry => new ConsumerPriceV2
                            {
                                DeliveryStart = entry.DeliveryStart,
                                DeliveryEnd = entry.DeliveryEnd,
                                EntryPerArea = entry.EntryPerArea
                            }).ToList();
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Log error
            Console.WriteLine($"Error fetching prices: {ex.Message}");
        }


        List<SpotPrice> spotPrice = new List<SpotPrice>();

        foreach (var item in consumerPrices)
        {
            TimeZoneInfo estoniaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. Europe Standard Time");

            DateTime estonianTime = TimeZoneInfo.ConvertTimeFromUtc(item.DeliveryStart, estoniaTimeZone);
            item.DeliveryStart = estonianTime;

            spotPrice.Add(new SpotPrice
            {
                DateTime = estonianTime,
                Rank = 0,
                PriceNoTax = Math.Round(item.EntryPerArea.Values.FirstOrDefault() / 1000, 5),
                PriceWithTax = Math.Round(item.EntryPerArea.Values.FirstOrDefault() / 1000, 5)
            });
        }

        return spotPrice;

    }
}
