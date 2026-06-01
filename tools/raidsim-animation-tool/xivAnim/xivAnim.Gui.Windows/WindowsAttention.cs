using System.Runtime.InteropServices;
using System.Windows.Interop;
using Eto.Forms;
using Form = Eto.Forms.Form;

namespace dev.susy_baka.xivAnim.Gui.Windows
{
    public static class WindowsAttention
    {
        private const uint FLASHW_STOP = 0;
        private const uint FLASHW_CAPTION = 1;
        private const uint FLASHW_TRAY = 2;
        private const uint FLASHW_ALL = FLASHW_CAPTION | FLASHW_TRAY;
        private const uint FLASHW_TIMERNOFG = 12;

        [StructLayout(LayoutKind.Sequential)]
        private struct FLASHWINFO
        {
            public uint cbSize;
            public IntPtr hwnd;
            public uint dwFlags;
            public uint uCount;
            public uint dwTimeout;
        }

        [DllImport("user32.dll")]
        private static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        public static void FlashUntilFocused(Form form)
        {
            var hwnd = GetHwnd(form);
            if (hwnd == IntPtr.Zero)
                return;

            var info = new FLASHWINFO
            {
                cbSize = (uint)Marshal.SizeOf<FLASHWINFO>(),
                hwnd = hwnd,
                dwFlags = FLASHW_ALL | FLASHW_TIMERNOFG,
                uCount = uint.MaxValue,
                dwTimeout = 0
            };

            FlashWindowEx(ref info);
        }

        public static void StopFlashing(Form form)
        {
            var hwnd = GetHwnd(form);
            if (hwnd == IntPtr.Zero)
                return;

            var info = new FLASHWINFO
            {
                cbSize = (uint)Marshal.SizeOf<FLASHWINFO>(),
                hwnd = hwnd,
                dwFlags = FLASHW_STOP,
                uCount = 0,
                dwTimeout = 0
            };

            FlashWindowEx(ref info);
        }

        private static IntPtr GetHwnd(Form form)
        {
            if (form.ToNative() is not System.Windows.Window nativeWindow)
                return IntPtr.Zero;

            var helper = new WindowInteropHelper(nativeWindow);

            // EnsureHandle is important because the HWND may not exist until needed.
            return helper.EnsureHandle();
        }
    }
}