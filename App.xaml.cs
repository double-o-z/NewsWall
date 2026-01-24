using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using NewsWall.Services;
using NewsWall.ViewModels;

namespace NewsWall;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        var services = new ServiceCollection();

        // Register services and viewmodels
        services.AddSingleton<IWebViewService, WebViewService>();
        services.AddSingleton<IPlatformAutomationService, PlatformAutomationService>();
        services.AddSingleton<MainViewModel>();

        _serviceProvider = services.BuildServiceProvider();

        var main = new Views.MainWindow();
        main.DataContext = _serviceProvider.GetRequiredService<MainViewModel>();
        main.Show();

        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (_serviceProvider is not null)
        {
            _serviceProvider.Dispose();
        }

        base.OnExit(e);
    }
}

