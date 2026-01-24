using Microsoft.Web.WebView2.Wpf;
using System.Threading.Tasks;

namespace NewsWall.Services;

public interface IStreamMonitorService
{
    /// <summary>
    /// Register a WebView2 instance for health monitoring. Optionally provide a health-check script
    /// that returns 1 when healthy and 0 when unhealthy. If <c>null</c>, the view will not be monitored.
    /// </summary>
    void RegisterView(WebView2 view, string? healthCheckScript = null);
    void UnregisterView(WebView2 view);
    Task CheckNowAsync();
}
