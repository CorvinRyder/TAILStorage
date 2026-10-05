using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ClipboardChecker
{
    class WinAPI
    {
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern int GetWindowTextW(IntPtr WindowsHandle, IntPtr TextPt, int Length);

        [DllImport("User32.dll")]
        public static extern bool AddClipboardFormatListener(IntPtr handler);

        [DllImport("User32.dll")]
        public static extern bool RemoveClipboardFormatListener(IntPtr handler);
    }
}
