using Microsoft.Web.WebView2.Wpf;
using DrawingPoint = System.Drawing.Point;
using System.Threading.Tasks;

namespace NewsWall.Services;

public class WebViewService : IWebViewService
{
    public async Task EnsureCoreWebView2Async(WebView2 view)
    {
        await view.EnsureCoreWebView2Async();
    }

    public async Task ExecuteScriptAsync(WebView2 view, string script)
    {
        await view.ExecuteScriptAsync(script);
    }

    public DrawingPoint GetWebView2ScreenPoint(WebView2 view, int offsetX, int offsetY)
    {
        var relative = view.PointToScreen(new System.Windows.Point(offsetX, offsetY));
        return new DrawingPoint((int)relative.X, (int)relative.Y);
    }

    public Task ReloadAsync(WebView2 view)
    {
        view.CoreWebView2?.Reload();
        return Task.CompletedTask;
    }
}
