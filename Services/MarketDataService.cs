using System;
using System.Net.Http;
using System.Text.Json;
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

    public async Task<IntelMarketData> GetIntelDataAsync()
    {
        try
        {
            var url = "https://query2.finance.yahoo.com/v8/finance/chart/INTC?range=1d&interval=1d&region=US&lang=en-US&includePrePost=true";
            var json = await Http.GetStringAsync(url);
            using var doc = JsonDocument.Parse(json);
            var meta = doc.RootElement
                .GetProperty("chart")
                .GetProperty("result")[0]
                .GetProperty("meta");

            decimal? regularPrice = null;
            decimal? preMarketPrice = null;
            decimal? postMarketPrice = null;

            if (meta.TryGetProperty("regularMarketPrice", out var regProp) && regProp.ValueKind == JsonValueKind.Number)
                regularPrice = regProp.GetDecimal();

            if (meta.TryGetProperty("fulldayPrice", out var preProp) && preProp.ValueKind == JsonValueKind.Number)
                preMarketPrice = preProp.GetDecimal();

            if (meta.TryGetProperty("fulldayPrice", out var postProp) && postProp.ValueKind == JsonValueKind.Number)
                postMarketPrice = postProp.GetDecimal();

            var session = DetermineMarketSession(meta);

            return new IntelMarketData(regularPrice, preMarketPrice, postMarketPrice, session);
        }
        catch
        {
            return new IntelMarketData(null, null, null, MarketSession.Closed);
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

    /// <summary>
    /// Determines the current US market session based on Eastern Time.
    /// </summary>
    private static MarketSession DetermineMarketSession(JsonElement meta)
    {
        // Get the exchange timezone offset from UTC (in seconds)
        int gmtOffsetSeconds = 0;
        if (meta.TryGetProperty("gmtoffset", out var offsetProp) && offsetProp.ValueKind == JsonValueKind.Number)
        {
            gmtOffsetSeconds = offsetProp.GetInt32();
        }

        // Current UTC time
        var utcNow = DateTime.UtcNow;

        // Convert to exchange local time
        var exchangeNow = utcNow.AddSeconds(gmtOffsetSeconds);

        // US markets (NYSE/NASDAQ) are in Eastern Time
        // Regular: 09:30 - 16:00 ET, Mon-Fri
        // Pre-market: 04:00 - 09:30 ET, Mon-Fri
        // After-market: 16:00 - 20:00 ET, Mon-Fri

        var dayOfWeek = exchangeNow.DayOfWeek;
        var time = exchangeNow.TimeOfDay;

        // Weekend → Closed
        if (dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday)
            return MarketSession.Closed;

        var preMarketStart = new TimeSpan(4, 0, 0);
        var regularStart = new TimeSpan(9, 30, 0);
        var regularEnd = new TimeSpan(16, 0, 0);
        var afterMarketEnd = new TimeSpan(20, 0, 0);

        if (time >= preMarketStart && time < regularStart)
            return MarketSession.PreMarket;

        if (time >= regularStart && time < regularEnd)
            return MarketSession.Regular;

        if (time >= regularEnd && time < afterMarketEnd)
            return MarketSession.AfterMarket;

        return MarketSession.Closed;
    }
}
