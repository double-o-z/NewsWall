using Microsoft.Web.WebView2.Wpf;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace NewsWall.Services;

public class StreamMonitorService : IStreamMonitorService, IDisposable
{
    private readonly ConcurrentDictionary<WebView2, byte> _views = new();
    private readonly Timer _timer;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(30);
    private readonly int _maxRetries = 3;

    public StreamMonitorService()
    {
        _timer = new Timer(async _ => await MonitorTickAsync(), null, _interval, _interval);
    }

    public void RegisterView(WebView2 view)
    {
        _views.TryAdd(view, 0);
    }

    public void UnregisterView(WebView2 view)
    {
        _views.TryRemove(view, out _);
    }

    public async Task CheckNowAsync()
    {
        await MonitorTickAsync();
    }

    private async Task MonitorTickAsync()
    {
        foreach (var kv in _views.Keys)
        {
            try
            {
                await CheckAndRecoverAsync(kv);
            }
            catch { }
        }
    }

    private async Task CheckAndRecoverAsync(WebView2 view)
    {
        // Interact with the WebView2 control on the UI thread via its Dispatcher.
        try
        {
            var op = view.Dispatcher.InvokeAsync(async () =>
            {
                // Basic heuristic: check if navigation state is ready and CoreWebView2 exists.
                var core = view.CoreWebView2;
                if (core == null)
                {
                    // attempt re-initialize by reloading the control
                    await ReloadViewAsync(view);
                    return;
                }

                // Try to check DOM health for a video element presence by executing a small script
                try
                {
                    var res = await view.ExecuteScriptAsync("(function(){const v=document.querySelector('video'); return v?1:0;})();");
                    if (res == "0")
                    {
                        // video not found - attempt reload
                        await ReloadViewAsync(view);
                    }
                }
                catch
                {
                    await ReloadViewAsync(view);
                }
            });

            // Wait for the dispatched async operation to complete
            await op.Task;
        }
        catch
        {
            // If dispatcher invocation failed, try a fallback reload on a ThreadPool thread
            try { await ReloadViewAsync(view); } catch { }
        }
    }

    private async Task ReloadViewAsync(WebView2 view)
    {
        int attempts = 0;
        while (attempts < _maxRetries)
        {
            try
            {
                view.Reload();
                return;
            }
            catch
            {
                attempts++;
                await Task.Delay(1000);
            }
        }

        // As a fallback, navigate to the same source again
        try
        {
            var src = view.Source?.ToString();
            if (!string.IsNullOrEmpty(src))
            {
                view.Source = new Uri(src);
            }
        }
        catch { }
    }

    public void Dispose()
    {
        _timer.Dispose();
    }
}
