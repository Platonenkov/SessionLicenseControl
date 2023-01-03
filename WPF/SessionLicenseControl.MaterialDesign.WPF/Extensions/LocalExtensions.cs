using System;
using System.Windows;
using System.Windows.Interop;

namespace SessionLicenseControl.WPF.Extensions
{
    internal static class LocalExtensions
    {
        public static void ForWindowFromTemplate(this object TemplateFrameworkElement, Action<Window> action)
        {
            if (((FrameworkElement)TemplateFrameworkElement).TemplatedParent is Window window) action(window);
        }

        public static IntPtr GetWindowHandle(this Window window)
        {
            var helper = new WindowInteropHelper(window);
            return helper.Handle;
        }

    }
}
