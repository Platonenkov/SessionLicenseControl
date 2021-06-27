using System;

namespace SessionLicenseControl.Interfaces
{
    public interface ILicenseBase
    {
        /// <summary> HDD id code </summary>
        public string HDDid { get; set; }
        /// <summary> License expiration time </summary>
        public DateTime? ExpirationDate { get; set; }
        public bool CheckSessions { get; set; }

        public string IssuedFor { get; set; }
    }
}