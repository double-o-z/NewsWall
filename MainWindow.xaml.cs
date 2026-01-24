using Microsoft.Web.WebView2.Wpf;
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            // c14View.CoreWebView2InitializationCompleted += (_, __) =>
            // {
            //     c14View.CoreWebView2.OpenDevToolsWindow();
            // };

            // i24View.CoreWebView2InitializationCompleted += (_, __) =>
            // {
            //     i24View.CoreWebView2.OpenDevToolsWindow();
            // };
        };
    }

    private System.Drawing.Point GetWebView2ScreenPoint(WebView2 view, int offsetX, int offsetY)
    {
        var relative = view.PointToScreen(new System.Windows.Point(offsetX, offsetY));
        return new System.Drawing.Point((int)relative.X, (int)relative.Y);
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        await c14View.EnsureCoreWebView2Async();

        c14View.CoreWebView2.NavigationCompleted += async (_, __) =>
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

            await c14View.ExecuteScriptAsync(js);
        };

        await i24View.EnsureCoreWebView2Async();

        i24View.CoreWebView2.NavigationCompleted += async (_, __) =>
        {
                // Wait for Flutter to initialize 
            await Task.Delay(5000);

            // Your calibrated offsets inside the WebView2 control
            int offsetX = 410;
            int offsetY = 175;

            // Convert to screen coordinates
            var screenPoint = GetWebView2ScreenPoint(i24View, offsetX, offsetY);

            // Send real OS-level click
            ClickScreen(screenPoint.X, screenPoint.Y);

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

            await i24View.ExecuteScriptAsync(muteJs);
        };
    }
}