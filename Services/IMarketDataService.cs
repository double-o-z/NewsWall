using System.Threading.Tasks;

namespace NewsWall.Services;

/// <summary>
/// Represents the current trading session for a US stock.
/// </summary>
public enum MarketSession
{
    PreMarket,    // 04:00 - 09:30 ET
    Regular,      // 09:30 - 16:00 ET
    AfterMarket,  // 16:00 - 20:00 ET
    Closed        // Weekends / overnight
}

/// <summary>
/// Intel stock market data including regular, pre-market, and after-market prices.
/// </summary>
public record IntelMarketData(
    decimal? RegularPrice,
    decimal? PreMarketPrice,
    decimal? PostMarketPrice,
    MarketSession Session
);

public interface IMarketDataService
{
    Task<IntelMarketData> GetIntelDataAsync();
    Task<decimal?> GetBtcPriceAsync();
    Task<int?> GetStrikeRadarPercentageAsync();
}
