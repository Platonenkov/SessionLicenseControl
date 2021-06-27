using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace SessionLicenseControl.WPF.Extensions
{
    public static class EventHandlerExtensions
    {
        public static void Invoke(this PropertyChangedEventHandler Event, object sender, string PropertyName) => Event?.Invoke(sender, new PropertyChangedEventArgs(PropertyName));
        public static void ThreadSafeInvoke(this PropertyChangedEventHandler Event, object sender, params string[] PropertyName)
        {
            if (Event is null) return;
            if (PropertyName is null || PropertyName.Length == 0) throw new ArgumentNullException(nameof(PropertyName));
            var args = PropertyName.Select(name => new PropertyChangedEventArgs(name)).ToArray();
            foreach (var d in Event.GetInvocationList())
                switch (d.Target)
                {
                    case DispatcherObject dispatcher_object when !dispatcher_object.CheckAccess():
                        foreach (var arg in args)
                            dispatcher_object.Dispatcher.Invoke(d, sender, arg);
                        break;
                    case ISynchronizeInvoke synchronize_invoke when synchronize_invoke.InvokeRequired:
                        foreach (var arg in args)
                            synchronize_invoke.Invoke(d, new[] { sender, arg });
                        break;
                    default:
                        foreach (var arg in args)
                            d.DynamicInvoke(sender, arg);
                        break;
                }
        }

    }
}
