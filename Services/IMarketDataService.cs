using System.Threading.Tasks;

namespace NewsWall.Services;

public interface IMarketDataService
{
    Task<decimal?> GetIntelPriceAsync();
    Task<decimal?> GetBtcPriceAsync();
    Task<int?> GetStrikeRadarPercentageAsync();
}
