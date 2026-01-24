# NewsWall

NewsWall is a lightweight WPF application that displays curated live news streams and web-based news widgets in a fullscreen, kiosk-style view. It uses Microsoft `WebView2` to render web content and includes small automation helpers to improve the presentation (for example: auto-fullscreening embedded players and muting audio on load).

## Key points

- Built with .NET 10 and C# 14
- WPF desktop application using `WebView2` for modern web rendering
- Designed for unattended fullscreen display (kiosk/office signage)

## Getting started

1. Clone the repository.
2. Open the solution in Visual Studio (recommended) targeting .NET 10.
3. Restore NuGet packages and run the app. WebView2 runtime is required on the host system.

## What it aims to achieve

- Provide a stable fullscreen display of multiple live news sources
- Improve user experience by automating common interactions (fullscreen, mute)
- Be a simple foundation for digital signage and news-wall scenarios

## Planned and suggested future features

- Per-source configuration (URLs, click offsets, autoplay behavior)
- Multi-monitor and layout support (grids, zones, scheduled rotation)
- Remote management / simple web API for updating sources and schedules
- Better error handling and recovery for network or renderer failures
- Themable UI and lightweight overlay controls (clock, headlines)
- Accessibility improvements and screen-reader support
 
## Project structure changes and MVVM migration notes

- This repository has started a migration toward the Microsoft MVVM Toolkit (`CommunityToolkit.Mvvm`) and dependency injection. Initial scaffolding was added:
  - `ViewModels/MainViewModel.cs` — a placeholder viewmodel.
  - `Services/IWebViewService.cs` and `Services/WebViewService.cs` — abstraction for WebView2 operations.
  - `Services/IPlatformAutomationService.cs` and `Services/PlatformAutomationService.cs` — abstraction for OS-level automation.
  - DI wiring in `App.xaml.cs` (services and `MainViewModel` are registered and `MainWindow.DataContext` is resolved at startup).

Please note: the app's visible behavior is preserved; existing automation and initialization logic in `MainWindow.xaml.cs` remains active until it can be safely migrated into services and viewmodels.

## Recent updates (migration & reliability)

- Migrated toward MVVM and DI using `CommunityToolkit.Mvvm` and `Microsoft.Extensions.DependencyInjection`.
- WebView initialization and JS injection moved into `Services/IWebViewService` / `Services/WebViewService`.
  - JS logic for player interactions was extracted into `Resources/js/c14-fullscreen.js` and `Resources/js/i24-mute.js` and is executed by the service.
- Added `StreamMonitorService` (`IStreamMonitorService`) to periodically check WebView health and attempt recovery (soft reload or re-navigation).
- Added `PlatformPowerService` (`IPlatformPowerService`) to detect system resume (wake) and trigger immediate health checks.
- Introduced `IPlatformAutomationService` to encapsulate OS-level automation (clicks) so it can be replaced or mocked.
- A small test project was briefly created during development but removed to avoid build conflicts; tests will be reintroduced under a safe test strategy in a feature branch.

The goal of these changes is to make the app more resilient to frozen streams and power-cycle scenarios while preparing the codebase for further MVVM refactors and unit testing.

## Contributing

Contributions, issues and feature requests are welcome. Please open an issue or submit a pull request.

## License

See the `LICENSE` file for details (if present).

---

Signed,

Double OZ