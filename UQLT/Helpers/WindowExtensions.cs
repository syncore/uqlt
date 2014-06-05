using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace UQLT.Helpers
{
    /// <summary>
    /// A helper class that flashes a window.
    /// </summary>
    public static class WindowExtensions
    {
        #region Window Flashing API Stuff

        // Stop flashing. The system restores the window to its original state.
        private const UInt32 FLASHW_STOP = 0;

        // Flash the window caption.
        private const UInt32 FLASHW_CAPTION = 1;

        // Flash the taskbar button.
        private const UInt32 FLASHW_TRAY = 2;

        // Flash both the window caption and taskbar button.
        private const UInt32 FLASHW_ALL = 3;

        // Flash continuously, until the FLASHW_STOP flag is set.
        private const UInt32 FLASHW_TIMER = 4;

        // Flash continuously until the window comes to the foreground.
        private const UInt32 FLASHW_TIMERNOFG = 12;

        [StructLayout(LayoutKind.Sequential)]
        private struct FLASHWINFO
        {
            // The size of the structure in bytes.
            public UInt32 cbSize;

            // A Handle to the Window to be Flashed. The window can be either opened or minimized.
            public IntPtr hwnd;

            // The Flash Status.
            public UInt32 dwFlags;

            // Number of times to flash the window
            public UInt32 uCount;

            // The rate at which the Window is to be flashed, in milliseconds. If Zero, the function uses the default cursor blink rate.
            public UInt32 dwTimeout;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        #endregion Window Flashing API Stuff

        /// <summary>
        /// Flashes the window.
        /// </summary>
        /// <param name="win">The win.</param>
        /// <param name="count">The count.</param>
        public static void FlashWindow(this Window win, UInt32 count = UInt32.MaxValue)
        {
            //Don't flash if the window is active
            //if (win.IsActive)
            //{
            //return;
            //}

            WindowInteropHelper h = new WindowInteropHelper(win);

            FLASHWINFO info = new FLASHWINFO
            {
                hwnd = h.Handle,
                dwFlags = FLASHW_ALL | FLASHW_TIMER,
                uCount = count,
                dwTimeout = 0
            };

            info.cbSize = Convert.ToUInt32(Marshal.SizeOf(info));
            FlashWindowEx(ref info);
        }

        /// <summary>
        /// Stops flashing the window.
        /// </summary>
        /// <param name="win">The win.</param>
        public static void StopFlashingWindow(this Window win)
        {
            WindowInteropHelper h = new WindowInteropHelper(win);

            FLASHWINFO info = new FLASHWINFO();
            info.hwnd = h.Handle;
            info.cbSize = Convert.ToUInt32(Marshal.SizeOf(info));
            info.dwFlags = FLASHW_STOP;
            info.uCount = UInt32.MaxValue;
            info.dwTimeout = 0;

            FlashWindowEx(ref info);
        }
    }
}