using System;

namespace SessionLicenseControl.Exceptions
{
    public class LicenseExceptions:Exception
    {
        public string MethodName { get; }
        public LicenseExceptions(string message, string method_name, Exception e = null) : base(message,e)
        {
            MethodName = method_name;
        }
    }
}