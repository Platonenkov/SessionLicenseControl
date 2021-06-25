using System;
using System.Collections.Generic;
using SessionLicenseControl.Licenses;
using SessionLicenseControl.Session;

namespace SessionLicenseControl.Interfaces
{
    public interface ILicenseBase
    {
        /// <summary> HDD id code </summary>
        public string HDDid { get; set; }
        /// <summary> License expiration time </summary>
        public DateTime? ExpirationDate { get; set; }

    }
    public interface ILicense : ILicenseBase
    {

        public bool IsValid { get; }

        /// <summary> Validate license </summary>
        /// <returns>validation result</returns>
        public bool ValidateLicense();

        #region Cryptography

        public string Encrypt(string Secret);
        public virtual License Decrypt(string row, string Secret) => row.Decrypt<License>(true, Secret);

        #endregion
    }
    public interface ISession
    {
        /// <summary> Session collection </summary>
        public List<DaySessions> Sessions { get; set; }
    }

    public interface ISessionLicense : ISession, ILicense
    {

    }
}
