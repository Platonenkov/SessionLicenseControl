using MathCore.Annotations;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using SessionLicenseControl.WPF.Enums;
using SessionLicenseControl.WPF.Extensions;

namespace SessionLicenseControl.WPF
{
    public static class User32
    {
        private const string FileName = "user32.dll";

        [DllImport(FileName, CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(this IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        [DllImport(FileName, CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(this IntPtr hWnd, WM Msg, IntPtr wParam, IntPtr lParam);

        public static IntPtr SendMessage([NotNull] this Window window, WM Msg, SC wParam, IntPtr lParam = default) => SendMessage(window.GetWindowHandle(), (uint)Msg, (IntPtr)wParam, lParam == default ? (IntPtr)' ' : lParam);
    }
}
