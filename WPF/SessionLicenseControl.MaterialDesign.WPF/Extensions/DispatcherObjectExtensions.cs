using System;
using System.Windows.Threading;
using MathCore.Annotations;

namespace SessionLicenseControl.WPF.Extensions
{
    public static class DispatcherObjectExtensions
    {
        public static void Invoke<T>(
            [NotNull] this T obj,
            [NotNull] Action<T> action,
            DispatcherPriority priority = DispatcherPriority.Normal)
            where T : DispatcherObject =>
            obj.Dispatcher?.Invoke(action, priority, obj);


        [CanBeNull]
        public static TValue GetValue<TObject, TValue>(
            [NotNull] this TObject obj,
            [NotNull] Func<TObject, TValue> func,
            DispatcherPriority priority = DispatcherPriority.Normal)
            where TObject : DispatcherObject =>
            (TValue)obj.Dispatcher?.Invoke(func, priority, obj);
    }
}
