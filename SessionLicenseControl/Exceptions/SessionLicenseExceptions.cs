using System;

namespace SessionLicenseControl.Exceptions
{
    public class SessionLicenseExceptions:Exception
    {
        public string MethodName { get; }
        public SessionLicenseExceptions(string message, string method_name, Exception e = null) : base(message,e)
        {
            MethodName = method_name;
        }
    }
}