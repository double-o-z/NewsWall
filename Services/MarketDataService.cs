using System;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NewsWall.Services;

public class MarketDataService : IMarketDataService
{
    private static readonly HttpClient Http = new()
    {
        DefaultRequestHeaders =
        {
            { "User-Agent", "NewsWall/1.0" },
            { "Accept", "application/json, text/plain, */*" }
        }
    };

    public async Task<decimal?> GetIntelPriceAsync()
    {
        try
        {
            // Yahoo Finance quote API for INTC
            var url = "https://query2.finance.yahoo.com/v8/finance/chart/INTC?range=1d&interval=1d&region=US&lang=en-US";
            var json = await Http.GetStringAsync(url);
            using var doc = JsonDocument.Parse(json);
            var price = doc.RootElement
                .GetProperty("chart")
                .GetProperty("result")[0]
                .GetProperty("meta")
                .GetProperty("regularMarketPrice")
                .GetDecimal();
            return price;
        }
        catch
        {
            return null;
        }
    }

    public async Task<decimal?> GetBtcPriceAsync()
    {
        try
        {
            var url = "https://api.coingecko.com/api/v3/simple/price?ids=bitcoin&vs_currencies=usd";
            var json = await Http.GetStringAsync(url);
            using var doc = JsonDocument.Parse(json);
            var price = doc.RootElement
                .GetProperty("bitcoin")
                .GetProperty("usd")
                .GetDecimal();
            return price;
        }
        catch
        {
            return null;
        }
    }

    public async Task<int?> GetStrikeRadarPercentageAsync()
    {
        try
        {
            // Use the official Strike Radar API instead of scraping the JS-rendered page
            var json = await Http.GetStringAsync("https://api.usstrikeradar.com/api/data");
            using var doc = JsonDocument.Parse(json);
            var risk = doc.RootElement
                .GetProperty("total_risk")
                .GetProperty("risk")
                .GetInt32();
            return risk;
        }
        catch
        {
            return null;
        }
    }
}
