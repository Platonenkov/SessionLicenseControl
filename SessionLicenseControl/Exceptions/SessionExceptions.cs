using System;

namespace SessionLicenseControl.Exceptions
{
    public class SessionExceptions:Exception
    {
        public string MethodName { get; }
        public SessionExceptions(string message, string method_name, Exception e = null) : base(message,e)
        {
            MethodName = method_name;
        }
    }
}
