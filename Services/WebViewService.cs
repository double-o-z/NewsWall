using Microsoft.Web.WebView2.Wpf;
using DrawingPoint = System.Drawing.Point;
using System.Threading.Tasks;

namespace NewsWall.Services;

public class WebViewService : IWebViewService
{
    private readonly System.Collections.Concurrent.ConcurrentDictionary<WebView2, byte> _initialized = new();

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

    public async Task InitializeC14Async(WebView2 view)
    {
        // Avoid registering handlers multiple times for the same view
        if (!_initialized.TryAdd(view, 0))
        {
            return;
        }

        await EnsureCoreWebView2Async(view);

        // Load the external JS resource once
        string? js = null;
        try
        {
            var exePath = System.AppDomain.CurrentDomain.BaseDirectory;
            var jsPath = System.IO.Path.Combine(exePath, "Resources", "js", "c14-fullscreen.js");
            if (System.IO.File.Exists(jsPath))
            {
                js = await System.IO.File.ReadAllTextAsync(jsPath);
            }
        }
        catch { js = null; }

        // Attach handler so the script is injected on navigation as well
        view.CoreWebView2.NavigationCompleted += async (_, __) =>
        {
            if (js is null) return;
            try { await ExecuteScriptAsync(view, js); } catch { }
        };

        // Also attempt to inject immediately (setInterval inside the script will retry until ready)
        if (js is not null)
        {
            try { await ExecuteScriptAsync(view, js); } catch { }
        }
    }

    public async Task InitializeI24Async(WebView2 view)
    {
        // Avoid re-injecting the mute script on every navigation; inject once only.
        if (!_initialized.TryAdd(view, 0))
        {
            return;
        }

        await EnsureCoreWebView2Async(view);

        // Wait for Flutter to initialize
        await Task.Delay(5000);

        // Your calibrated offsets inside the WebView2 control
        int offsetX = 410;
        int offsetY = 175;

        // Convert to screen coordinates
        var screenPoint = GetWebView2ScreenPoint(view, offsetX, offsetY);

        // Use platform automation if available
        var platform = App.ServiceProvider?.GetService(typeof(IPlatformAutomationService)) as IPlatformAutomationService;
        if (platform != null)
        {
            platform.ClickScreen(screenPoint.X, screenPoint.Y);
        }
        else
        {
            System.Windows.Forms.Cursor.Position = new System.Drawing.Point(screenPoint.X, screenPoint.Y);
        }

        // Inject the mute script once. The interval inside the script retries until
        // the video is ready, mutes once, then stops. It is not re-injected on
        // subsequent navigations so a manual unmute sticks.
        try
        {
            var exePath = System.AppDomain.CurrentDomain.BaseDirectory;
            var jsPath = System.IO.Path.Combine(exePath, "Resources", "js", "i24-mute.js");
            if (System.IO.File.Exists(jsPath))
            {
                var js = await System.IO.File.ReadAllTextAsync(jsPath);
                await ExecuteScriptAsync(view, js);
            }
        }
        catch { }
    }
}
