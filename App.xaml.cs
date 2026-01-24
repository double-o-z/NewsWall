using System;
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
    public static IServiceProvider? ServiceProvider { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        var services = new ServiceCollection();

        // Register services and viewmodels
        services.AddSingleton<IWebViewService, WebViewService>();
        services.AddSingleton<IPlatformAutomationService, PlatformAutomationService>();
        services.AddSingleton<IStreamMonitorService, StreamMonitorService>();
        services.AddSingleton<IPlatformPowerService, PlatformPowerService>();
        services.AddSingleton<MainViewModel>();

        _serviceProvider = services.BuildServiceProvider();
        ServiceProvider = _serviceProvider;

        var main = new Views.MainWindow();
        main.DataContext = _serviceProvider.GetRequiredService<MainViewModel>();
        // Start power service so it can listen for resume events
        var power = _serviceProvider.GetRequiredService<IPlatformPowerService>();
        power.Start();
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

