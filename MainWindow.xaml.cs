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
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Delegate initialization and JS injection to the IWebViewService via MainViewModel
        if (DataContext is ViewModels.MainViewModel vm)
        {
            await vm.InitializeWebViewsAsync(c14View, i24View);
        }
        else
        {
            // Fallback to previous behavior if ViewModel is not available
            await c14View.EnsureCoreWebView2Async();
            await i24View.EnsureCoreWebView2Async();
        }
    }
}