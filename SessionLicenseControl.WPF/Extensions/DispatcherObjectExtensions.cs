using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using MathCore.Annotations;

namespace SessionLicenseControl.WPF.Extensions
{
    public static class DispatcherObjectExtensions
    {
        public static T Invoke<T>(this Dispatcher dispatcher, Func<T> Selector) => dispatcher.Invoke(Selector, DispatcherPriority.Normal);

        public static void Invoke<T>(
            [NotNull] this T obj,
            [NotNull] Action<T> action,
            DispatcherPriority priority = DispatcherPriority.Normal)
            where T : DispatcherObject =>
            obj.Dispatcher?.Invoke(action, priority, obj);

        public static Task InvokeAsync<T>(
            [NotNull] this T obj,
            [NotNull] Action<T> action,
            DispatcherPriority priority = DispatcherPriority.Normal)
            where T : DispatcherObject =>
            obj.Dispatcher?.BeginInvoke(action, priority, obj).Task;


        [CanBeNull]
        public static TValue GetValue<TObject, TValue>(
            [NotNull] this TObject obj,
            [NotNull] Func<TObject, TValue> func,
            DispatcherPriority priority = DispatcherPriority.Normal)
            where TObject : DispatcherObject =>
            (TValue)obj.Dispatcher?.Invoke(func, priority, obj);

        [CanBeNull]
        public static Task<TValue> GetValueAsync<TObject, TValue>(
            [NotNull] this TObject obj,
            [NotNull] Func<TObject, TValue> func,
            DispatcherPriority priority = DispatcherPriority.Normal)
            where TObject : DispatcherObject =>
            (Task<TValue>)obj.Dispatcher?.BeginInvoke(func, priority, obj).Task;

    }
}
