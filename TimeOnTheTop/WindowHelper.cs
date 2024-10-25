using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace TimeOnTheTop;

#pragma warning disable all
public static partial class WindowHelper
{
    public static IntPtr GetHandle(Window window) => new WindowInteropHelper(window).Handle;

    // Window ExTransparent

    public const int WS_EX_TRANSPARENT = 0x00000020;
    public const int GWL_EXSTYLE = -20;

    [DllImport("user32.dll")]
    public static extern int GetWindowLong(IntPtr hWnd, int index);

    [DllImport("user32.dll")]
    public static extern int SetWindowLong(IntPtr hWnd, int index, int newStyle);

    public static void SetWindowExTransparent(IntPtr hWnd)
    {
        var extendedStyle = GetWindowLong(hWnd, GWL_EXSTYLE);
        SetWindowLong(hWnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);
    }

    // App System Theme

    public const int APPMODE_LIGHT = 1;
    public const int APPMODE_DARK = 2;

    [DllImport("uxtheme.dll", EntryPoint = "#135", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern int SetPreferredAppMode(int preferredAppMode);

    [DllImport("uxtheme.dll", EntryPoint = "#136", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern void FlushMenuThemes();

    public const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
    public const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

    [DllImport("dwmapi.dll")]
    public static extern int DwmSetWindowAttribute(IntPtr hWnd, int attr, ref int attrValue, int attrSize);

    public static bool SetWindowFrameworkDarkMode(IntPtr hWnd, bool isDarkMode)
    {
        if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17763))
        {
            var attribute = DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1;
            if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 18985))
            {
                attribute = DWMWA_USE_IMMERSIVE_DARK_MODE;
            }

            int useImmersiveDarkMode = isDarkMode ? 1 : 0;
            return DwmSetWindowAttribute(hWnd, attribute, ref useImmersiveDarkMode, sizeof(int)) == 0;
        }

        return false;
    }
}
