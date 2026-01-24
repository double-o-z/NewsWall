using System.Windows;

namespace NewsWall.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        WindowStyle = WindowStyle.None;
        WindowState = WindowState.Maximized;
        Loaded += (_, __) =>
        {
        };

        // Ensure we unregister views when window closes
        Closed += (_, __) =>
        {
            var monitor = NewsWall.App.ServiceProvider?.GetService(typeof(NewsWall.Services.IStreamMonitorService)) as NewsWall.Services.IStreamMonitorService;
            if (monitor != null)
            {
                try { monitor.UnregisterView(c14View); } catch { }
                try { monitor.UnregisterView(i24View); } catch { }
            }
        };
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Delegate initialization and JS injection to the IWebViewService via MainViewModel
        if (DataContext is ViewModels.MainViewModel vm)
        {
            await vm.InitializeWebViewsAsync(c14View, i24View);

            // After initializing webviews, register them with the stream monitor so they can be health-checked
            var monitor = NewsWall.App.ServiceProvider?.GetService(typeof(NewsWall.Services.IStreamMonitorService)) as NewsWall.Services.IStreamMonitorService;
            if (monitor != null)
            {
                try { monitor.RegisterView(c14View, "(function(){const iframe=document.getElementById('univ-live-iframe'); return iframe?1:0;})();"); } catch { }
                try { monitor.RegisterView(i24View, "(function(){const v=document.querySelector('video'); return v?1:0;})();"); } catch { }
                // perform an immediate check on startup
                _ = monitor.CheckNowAsync();
            }
        }
        else
        {
            // Fallback to previous behavior if ViewModel is not available
            await c14View.EnsureCoreWebView2Async();
            await i24View.EnsureCoreWebView2Async();
        }
    }
}