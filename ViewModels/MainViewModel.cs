using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NewsWall.Services;
using System.Threading.Tasks;

namespace NewsWall.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IWebViewService _webViewService;
    private readonly IPlatformAutomationService _platformAutomationService;

    public IAsyncRelayCommand InitializeCommand { get; }

    public MainViewModel(IWebViewService webViewService, IPlatformAutomationService platformAutomationService)
    {
        _webViewService = webViewService;
        _platformAutomationService = platformAutomationService;

        InitializeCommand = new AsyncRelayCommand(InitializeAsync);
    }

    private async Task InitializeAsync()
    {
        // Placeholder for initialization logic moved from code-behind in future iterations.
        await Task.CompletedTask;
    }
}
