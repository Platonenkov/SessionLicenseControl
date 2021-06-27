using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using SessionLicenseControl.WPF.Enums;

namespace SessionLicenseControl.WPF.Classes
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto), SuppressMessage("ReSharper", "InconsistentNaming")]
    public class MONITORINFO
    {
        /// <summary>
        /// </summary>            
        public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));

        /// <summary>
        /// </summary>            
        public RECT rcMonitor = new();

        /// <summary>
        /// </summary>            
        public RECT rcWork = new();

        /// <summary>
        /// </summary>            
        public int dwFlags = 0;
    }
}