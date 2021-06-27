using System;
using SessionLicenseControl.Licenses;

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
    public interface ILicense : ILicenseBase
    {
        public bool IsValid { get; }

        /// <summary> Validate license </summary>
        /// <returns>validation result</returns>
        public bool ValidateLicense();

        #region Cryptography

        public string Encrypt(string Secret);
        public virtual License Decrypt(string row, string Secret) => row.DecryptRow<License>(Secret);

        #endregion
    }

}
