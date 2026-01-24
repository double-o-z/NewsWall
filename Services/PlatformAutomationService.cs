using System;
using System.Runtime.InteropServices;
using System.Drawing;

namespace NewsWall.Services;

public class PlatformAutomationService : IPlatformAutomationService
{
    [DllImport("user32.dll")]
    static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);

    const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
    const uint MOUSEEVENTF_LEFTUP = 0x0004;

    public void ClickScreen(int x, int y)
    {
        System.Windows.Forms.Cursor.Position = new System.Drawing.Point(x, y);
        mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
        mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);
    }
}
