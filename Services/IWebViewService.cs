using Microsoft.Web.WebView2.Wpf;
using System.Drawing;
using System.Threading.Tasks;

namespace NewsWall.Services;

public interface IWebViewService
{
    Task EnsureCoreWebView2Async(WebView2 view);
    Task ExecuteScriptAsync(WebView2 view, string script);
    System.Drawing.Point GetWebView2ScreenPoint(WebView2 view, int offsetX, int offsetY);
    Task ReloadAsync(WebView2 view);
}
