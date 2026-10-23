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

        // --- Step 1: Select the Hebrew / i24 language ---
        // Wait for the Flutter language-selection page to initialize.
        await Task.Delay(5000);

        // Click the Hebrew / i24 language button (bottom-right of the language selector).
        // These offsets are relative to the WebView2 control's top-left corner.
        // Calibrate them to match the red-rectangle button in Image 1.
        int langOffsetX = 430;   // TODO: adjust to the Hebrew button's X position
        int langOffsetY = 620;   // TODO: adjust to the Hebrew button's Y position

        var langPoint = GetWebView2ScreenPoint(view, langOffsetX, langOffsetY);
        ClickScreenPoint(langPoint);

        // --- Step 2: Wait for the stream to load ---
        await Task.Delay(8000); // 5-10 seconds for the video player to appear

        // --- Step 3: Click the Play ("הפעלה") button ---
        // Click the red play button that appears after the stream loads.
        // Calibrate these to match the red-rectangle button in Image 2.
        int playOffsetX = 620;   // TODO: adjust to the Play button's X position
        int playOffsetY = 440;   // TODO: adjust to the Play button's Y position

        var playPoint = GetWebView2ScreenPoint(view, playOffsetX, playOffsetY);
        ClickScreenPoint(playPoint);

        // --- Step 4: Inject the mute script once ---
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

    private void ClickScreenPoint(System.Drawing.Point point)
    {
        var platform = App.ServiceProvider?.GetService(typeof(IPlatformAutomationService)) as IPlatformAutomationService;
        if (platform != null)
        {
            platform.ClickScreen(point.X, point.Y);
        }
        else
        {
            System.Windows.Forms.Cursor.Position = point;
        }
    }
}
