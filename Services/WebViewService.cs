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

    public async Task InitializeC14Async(WebView2 view)
    {
        await EnsureCoreWebView2Async(view);

        view.CoreWebView2.NavigationCompleted += async (_, __) =>
        {
            string js = @"
                console.log('C14 fullscreen script injected');

                let attempts = 0;

                const tryFull = () => {
                    attempts++;
                    console.log('Attempt', attempts);

                    const iframe = document.getElementById('univ-live-iframe');
                    if (!iframe) {
                        console.log('iframe not found');
                        return;
                    }

                    const doc = iframe.contentWindow?.document;
                    if (!doc) {
                        console.log('iframe document not ready');
                        return;
                    }

                    const btn = doc.querySelector('.rgpl-btn-fullscreen');
                    if (!btn) {
                        console.log('fullscreen button not found');
                        return;
                    }

                    console.log('Clicking fullscreen');
                    btn.click();
                    clearInterval(window._c14FullInt);
                };

                window._c14FullInt = setInterval(tryFull, 500);
            ";

            await ExecuteScriptAsync(view, js);
        };
    }

    public async Task InitializeI24Async(WebView2 view)
    {
        await EnsureCoreWebView2Async(view);

        view.CoreWebView2.NavigationCompleted += async (_, __) =>
        {
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

            // Now wait for the video and mute it
            string muteJs = @"
                const tryMute = () => {
                    const vid = document.querySelector('video');
                    if (vid) {
                        vid.muted = true;
                        vid.volume = 0;
                        clearInterval(window._muteInt);
                    }
                };
                window._muteInt = setInterval(tryMute, 500);
            ";

            await ExecuteScriptAsync(view, muteJs);
        };
    }
}
