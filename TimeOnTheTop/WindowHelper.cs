using System.Runtime.InteropServices;

namespace TimeOnTheTop
{
    public static partial class WindowHelper
    {
        const int WS_EX_TRANSPARENT = 0x00000020;
        const int GWL_EXSTYLE = -20;

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int index);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int index, int newStyle);

        public static void SetWindowExTransparent(IntPtr hWnd)
        {
            var extendedStyle = GetWindowLong(hWnd, GWL_EXSTYLE);
            SetWindowLong(hWnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);
        }
    }
}
