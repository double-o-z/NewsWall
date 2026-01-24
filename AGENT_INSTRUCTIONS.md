Agent instructions — Align NewsWall with Microsoft MVVM Toolkit and WPF best practices

Purpose
- Provide a concise, actionable plan for future agentic work to migrate and align the project with the Microsoft `CommunityToolkit.Mvvm` (MVVM Toolkit) and general WPF design principles. The goal is a cleaner, better organized codebase that is testable and ready for new features.

High-level goals
- Replace UI logic in code-behind with `ViewModel`s and `ICommand` implementations.
- Establish a predictable folder structure and naming conventions.
- Introduce dependency injection for services and platform concerns.
- Add lightweight testing strategy for `ViewModel`s and services.
- Prepare CI checks for build and unit tests.

Recommended project layout
- `src/` (application projects)
  - `NewsWall.UI` (WPF app)
    - `Views/` (XAML views)
    - `ViewModels/` (VM classes)
    - `Services/` (abstractions and implementations for navigation, settings, webview helpers)
    - `Resources/` (ResourceDictionaries, styles)
- `tests/`
  - `NewsWall.UnitTests` (unit tests for viewmodels and services)

Initial migration tasks (priority order)
1. Add `CommunityToolkit.Mvvm` NuGet package and `Microsoft.Extensions.DependencyInjection`.
2. Create base classes: `ObservableObjectBase` (or use `ObservableObject`) and `ViewModelBase` where common DI and cancellation tokens live.
3. Move logic out of `MainWindow.xaml.cs`:
   - Create `MainViewModel` that exposes `ICommand`/`IAsyncRelayCommand` for actions currently in code-behind (initialization, WebView2 handling).
   - Introduce a `IWebViewService` to encapsulate WebView2 interactions (initialization, ExecuteScript, coordinate conversion) so ViewModels call a service rather than direct UI APIs.
4. Replace direct OS-level calls and automation code with a platform service (e.g. `IPlatformAutomationService`) that can be stubbed in tests.
5. Wire up DI in `App.xaml.cs` or `Program.cs` (if using new startup pattern) and register ViewModels and services as singletons/transients as appropriate.

XAML and data templates
- Use `DataTemplate` to map `ViewModel` -> `View` instead of manually constructing views.
- Keep `Views` minimal: only UI and binding declarations. Avoid event handlers that contain business logic.
- Use `Binding` with `Mode`, `UpdateSourceTrigger`, and `Converters` where needed.

Commands, async and cancellation
- Prefer `IAsyncRelayCommand` for async work and support cancellation tokens.
- Expose `IsBusy` on ViewModels to allow UI to show progress and prevent duplicate execution.

Testing
- Add unit tests for ViewModels and services with DI and mocked platform services.
- Focus tests on behavior: command execution, state transitions, and script composition logic.

CI and quality
- Add GitHub Actions to build solution and run tests on PRs.
- Add simple linter/formatting checks (dotnet format) and prefer small PRs for each migration step.

Developer conventions
- Commit message prefix: `feat:`, `fix:`, `refactor:`, `docs:`, `test:`.
- Keep PRs focused and document design decisions in short README updates.

Accessibility, logging and resilience
- Add logging (e.g. `Serilog`) for runtime errors and key state transitions.
- Add graceful recovery for WebView2 failures and network issues.
- Consider focus management and screen reader hints for signage overlays.

Agentic task examples (ready-to-run tasks)
- "Create `IWebViewService` and replace direct `WebView2` calls in `MainWindow` with service calls and a `MainViewModel`." 
- "Introduce DI container in `App.xaml.cs` and register `MainViewModel` and services." 
- "Add unit tests for `MainViewModel` command behavior mocking `IWebViewService`."


---

Iteration workflow for agents
- Each agent run should pick one or more tasks from the "Tasks to perform" list and attempt to carry them out.
- After the attempt, the agent must update this file:
  - If the task completed successfully, move it into the "Tasks successfully performed" list with a short one-line note and date.
  - If the task was attempted but not completed, move an explanatory entry into the "Tasks successfully performed" list describing what was attempted and why it failed, and update the remaining work in the "Tasks to perform" list so it reflects the current next steps.
  - Always keep the "Tasks to perform" list limited to at most 5 actionable items for the next agent run.
- Agents must make a Git commit with their changes and push to the repository branch used for development.

Tasks to perform (up to 5)
1. Add `CommunityToolkit.Mvvm` and `Microsoft.Extensions.DependencyInjection` NuGet packages to the WPF project and verify the solution builds.
2. Scaffold `ViewModelBase` (inheriting from `ObservableObject`) and create an initial `MainViewModel` with a placeholder `InitializeAsync` command.
3. Create `IWebViewService` abstraction and a simple implementation that wraps `WebView2` calls used today (EnsureCoreWebView2Async, ExecuteScriptAsync, point conversion). Register it with DI as a scoped/singleton service.
4. Implement stream recovery and power-resume handling:
   - Create `IStreamMonitorService` (or `IWebViewHealthService`) responsible for periodically checking each WebView instance for a frozen or unresponsive stream (e.g. no frame progress, missing expected DOM elements, or no audio/video activity). Provide a configurable polling interval and retry policy.
   - Implement restart behavior inside the monitor (soft-reload via `CoreWebView2.Reload()` or re-navigation; fallback to re-initialization of the WebView if reload fails). Expose an API so callers (or the system power service) can request an immediate restart.
   - Add unit tests for the monitor by mocking `IWebViewService` to simulate frozen and healthy states and asserting restart attempts.
5. Detect system sleep/wake and trigger recovery:
   - Introduce `IPlatformPowerService` to receive OS power events (resume from sleep). On resume, call the `IStreamMonitorService` to perform an immediate health check and restart streams as needed.
   - Ensure the service is registered in DI and that the app subscribes/unsubscribes to OS events safely (avoid leaks). Add a small integration test plan to manually verify behavior on suspend/resume.

Tasks successfully performed
- None yet.


Acceptance criteria for stream recovery feature
- The app periodically checks WebView stream health without blocking the UI thread.
- When a frozen or unresponsive stream is detected, the app attempts a graceful reload and, if necessary, reinitializes the WebView.
- On system resume from sleep, the app triggers an immediate health check and restarts affected streams.
- The monitor and power services are abstracted behind interfaces and registered with DI so they can be unit tested.
- Unit tests cover the monitor restart logic (using mocked `IWebViewService`) and basic retry scenarios.

Notes
- Keep changes small and verifiable; each agent run should produce a passing build if possible.
- When running work that modifies XAML, prefer incremental commits and run the app to validate UI behavior locally.

---

If this matches your expectations, I will save and push this change now.