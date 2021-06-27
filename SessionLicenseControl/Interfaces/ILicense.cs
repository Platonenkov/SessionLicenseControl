using System;
using SessionLicenseControl.Licenses;

namespace SessionLicenseControl.Interfaces
{
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
