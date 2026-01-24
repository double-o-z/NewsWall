using Microsoft.Web.WebView2.Wpf;
using System.Threading.Tasks;

namespace NewsWall.Services;

public interface IStreamMonitorService
{
    void RegisterView(WebView2 view);
    void UnregisterView(WebView2 view);
    Task CheckNowAsync();
}
