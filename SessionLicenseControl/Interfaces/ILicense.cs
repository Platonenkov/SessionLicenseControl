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
        public License Decrypt(string row, string Secret);

        #endregion
    }

}
