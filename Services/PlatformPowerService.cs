using Microsoft.Win32;
using System;

namespace NewsWall.Services;

public class PlatformPowerService : IPlatformPowerService, IDisposable
{
    private readonly IStreamMonitorService _monitor;

    public PlatformPowerService(IStreamMonitorService monitor)
    {
        _monitor = monitor;
    }

    public void Start()
    {
        SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
    }

    public void Stop()
    {
        SystemEvents.PowerModeChanged -= SystemEvents_PowerModeChanged;
    }

    private void SystemEvents_PowerModeChanged(object? sender, PowerModeChangedEventArgs e)
    {
        if (e.Mode == PowerModes.Resume)
        {
            // trigger an immediate check and recovery
            _ = _monitor.CheckNowAsync();
        }
    }

    public void Dispose()
    {
        Stop();
    }
}
