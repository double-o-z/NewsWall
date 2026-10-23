﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;
using NewsWall.Services;

namespace NewsWall.Views;

/// <summary>
/// Interaction logic for MainWindow
/// </summary>
public partial class MainWindow : Window
{
    private readonly IMarketDataService _marketDataService;
    private readonly IRotterNewsService _rotterNewsService;
    private readonly DispatcherTimer _marketTimer;
    private readonly DispatcherTimer _rotterTimer;

    public MainWindow()
    {
        InitializeComponent();

        WindowStyle = WindowStyle.None;
        WindowState = WindowState.Maximized;
        Loaded += (_, __) => { };

        _marketDataService = App.ServiceProvider?.GetService(typeof(IMarketDataService)) as IMarketDataService
                             ?? throw new InvalidOperationException("IMarketDataService not registered");
        _rotterNewsService = App.ServiceProvider?.GetService(typeof(IRotterNewsService)) as IRotterNewsService
                             ?? throw new InvalidOperationException("IRotterNewsService not registered");

        _marketTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(60) };
        _marketTimer.Tick += async (_, _) => await RefreshMarketDataAsync();

        _rotterTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(120) };
        _rotterTimer.Tick += async (_, _) => await RefreshRotterNewsAsync();

        Closed += (_, __) =>
        {
            _marketTimer.Stop();
            _rotterTimer.Stop();
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
        if (DataContext is ViewModels.MainViewModel vm)
        {
            await vm.InitializeWebViewsAsync(c14View, i24View);
            var monitor = NewsWall.App.ServiceProvider?.GetService(typeof(NewsWall.Services.IStreamMonitorService)) as NewsWall.Services.IStreamMonitorService;
            if (monitor != null)
            {
                try { monitor.RegisterView(c14View, "(function(){const iframe=document.getElementById('univ-live-iframe'); return iframe?1:0;})();"); } catch { }
                try { monitor.RegisterView(i24View, "(function(){const v=document.querySelector('video'); return v?1:0;})();"); } catch { }
                _ = monitor.CheckNowAsync();
            }
        }
        else
        {
            await c14View.EnsureCoreWebView2Async();
            await i24View.EnsureCoreWebView2Async();
        }

        _ = RefreshMarketDataAsync();
        _ = RefreshRotterNewsAsync();
        _marketTimer.Start();
        _rotterTimer.Start();
    }

    private void BtnEnlargeLeft_Click(object sender, RoutedEventArgs e) => SetEnlargedStream(left: true);
    private void BtnEnlargeRight_Click(object sender, RoutedEventArgs e) => SetEnlargedStream(left: false);
    private void BtnRestore_Click(object sender, RoutedEventArgs e) => RestoreSplitLayout();

    private void SetEnlargedStream(bool left)
    {
        if (left)
        {
            ColLeft.Width = new GridLength(1, GridUnitType.Star);
            ColRight.Width = new GridLength(0);
            i24View.Visibility = Visibility.Collapsed;
            c14View.Visibility = Visibility.Visible;
        }
        else
        {
            ColLeft.Width = new GridLength(0);
            ColRight.Width = new GridLength(1, GridUnitType.Star);
            c14View.Visibility = Visibility.Collapsed;
            i24View.Visibility = Visibility.Visible;
        }
        btnEnlargeLeft.Visibility = Visibility.Collapsed;
        btnEnlargeRight.Visibility = Visibility.Collapsed;
        btnRestore.Visibility = Visibility.Visible;
    }

    private void RestoreSplitLayout()
    {
        ColLeft.Width = new GridLength(1, GridUnitType.Star);
        ColRight.Width = new GridLength(1, GridUnitType.Star);
        c14View.Visibility = Visibility.Visible;
        i24View.Visibility = Visibility.Visible;
        btnEnlargeLeft.Visibility = Visibility.Visible;
        btnEnlargeRight.Visibility = Visibility.Visible;
        btnRestore.Visibility = Visibility.Collapsed;
    }
    private async Task RefreshMarketDataAsync()
    {
        try
        {
            var intel = await _marketDataService.GetIntelPriceAsync();
            Dispatcher.Invoke(() =>
            {
                txtIntelPrice.Text = intel.HasValue ? $"INTC: ${intel.Value:F2}" : "INTC: --";
            });
        }
        catch { }

        try
        {
            var btc = await _marketDataService.GetBtcPriceAsync();
            Dispatcher.Invoke(() =>
            {
                txtBtcPrice.Text = btc.HasValue ? $"BTC: ${btc.Value:N0}" : "BTC: --";
            });
        }
        catch { }

        try
        {
            var strike = await _marketDataService.GetStrikeRadarPercentageAsync();
            Dispatcher.Invoke(() =>
            {
                if (strike.HasValue)
                {
                    txtStrikeRadar.Text = $"Strike: {strike.Value}%";
                    SetStrikeRadarColor(strike.Value);
                }
                else
                {
                    txtStrikeRadar.Text = "Strike: --";
                    txtStrikeRadar.Foreground = new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(0xCC, 0xCC, 0xCC));
                }
            });
        }
        catch { }
    }

    private void SetStrikeRadarColor(int percentage)
    {
        var color = percentage switch
        {
            <= 30 => System.Windows.Media.Color.FromRgb(0x4C, 0xAF, 0x50),
            <= 60 => System.Windows.Media.Color.FromRgb(0xE8, 0xC5, 0x47),
            <= 80 => System.Windows.Media.Color.FromRgb(0xE8, 0x7E, 0x47),
            _     => System.Windows.Media.Color.FromRgb(0xE8, 0x47, 0x47),
        };
        txtStrikeRadar.Foreground = new System.Windows.Media.SolidColorBrush(color);
    }

    private async Task RefreshRotterNewsAsync()
    {
        try
        {
            var headlines = await _rotterNewsService.GetLatestHeadlinesAsync(count: 6);
            Dispatcher.Invoke(() =>
            {
                spRotterLeft.Children.Clear();
                spRotterRight.Children.Clear();

                if (headlines.Count > 0)
                {
                    // Split headlines between left and right columns.
                    // Hebrew reads right-to-left, so newest headlines go on the right.
                    var mid = (headlines.Count + 1) / 2;
                    var leftHeadlines = headlines.Skip(mid).ToList();   // older on the left
                    var rightHeadlines = headlines.Take(mid).ToList();  // newest on the right

                    AddHeadlinesToPanel(spRotterLeft, leftHeadlines);
                    AddHeadlinesToPanel(spRotterRight, rightHeadlines);
                }
                else
                {
                    var noDataLeft = new TextBlock
                    {
                        Text = "רוטר: לא הצליח לטעון כותרות",
                        Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xCC, 0xCC, 0xCC)),
                        FontSize = 12,
                        Margin = new Thickness(0, 4, 0, 4)
                    };
                    var noDataRight = new TextBlock
                    {
                        Text = "",
                        Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xCC, 0xCC, 0xCC)),
                        FontSize = 12,
                        Margin = new Thickness(0, 4, 0, 4)
                    };
                    spRotterLeft.Children.Add(noDataLeft);
                    spRotterRight.Children.Add(noDataRight);
                }
            });
        }
        catch
        {
            Dispatcher.Invoke(() =>
            {
                spRotterLeft.Children.Clear();
                spRotterRight.Children.Clear();
                var errLeft = new TextBlock
                {
                    Text = "רוטר: שגיאה בטעינת כותרות",
                    Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xCC, 0xCC, 0xCC)),
                    FontSize = 12,
                    Margin = new Thickness(0, 4, 0, 4)
                };
                var errRight = new TextBlock
                {
                    Text = "",
                    Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0xCC, 0xCC, 0xCC)),
                    FontSize = 12,
                    Margin = new Thickness(0, 4, 0, 4)
                };
                spRotterLeft.Children.Add(errLeft);
                spRotterRight.Children.Add(errRight);
            });
        }
    }

    private void AddHeadlinesToPanel(StackPanel panel, List<RotterHeadline> headlines)
    {
        foreach (var hl in headlines)
        {
            // Create a clickable Hyperlink inside a TextBlock
            var hyperlink = new Hyperlink
            {
                NavigateUri = new Uri(hl.Url),
                Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x4A, 0x90, 0xE2)),
                TextDecorations = System.Windows.TextDecorations.Underline
            };
            hyperlink.Inlines.Add(hl.Title);
            hyperlink.RequestNavigate += (s, e) =>
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = e.Uri.AbsoluteUri,
                        UseShellExecute = true
                    });
                }
                catch { }
                e.Handled = true;
            };

            var textBlock = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                FlowDirection = System.Windows.FlowDirection.RightToLeft,
                FontSize = 13,
                Margin = new Thickness(0, 3, 0, 3)
            };
            textBlock.Inlines.Add(hyperlink);

            panel.Children.Add(textBlock);
        }
    }
}
