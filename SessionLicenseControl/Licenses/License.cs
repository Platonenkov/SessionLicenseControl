using System;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MathCore.Annotations;
using SessionLicenseControl.Exceptions;
using SessionLicenseControl.Extensions;
using SessionLicenseControl.Information;
using SessionLicenseControl.Interfaces;

namespace SessionLicenseControl.Licenses
{
    public class License : ILicense
    {
        public const string LicenseDirectory = "License";
        public const string LicenseFileName = "License.lic";
        public const string DefaultIssuedFor = "everybody";

        public string HDDid { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string IssuedFor { get; set; }

        public bool IsValid => ValidateLicense();
        public bool CheckSessions { get; set; }
        public License()
        {

        }
        public License(string hdd, DateTime? ExpirationDate,string issued_for, bool check_sessions)
        {
            IssuedFor = issued_for;
            CheckSessions = check_sessions;
            HDDid = hdd;
            this.ExpirationDate = ExpirationDate;
        }

        public License(string row, string secret)
        {
            var input = this.Decrypt(row, secret);
            IssuedFor = input.IssuedFor;
            CheckSessions = input.CheckSessions;
            ExpirationDate = input.ExpirationDate;
            HDDid = input.HDDid;
        }

        public License(FileInfo file, string secret) => this.LoadFromFile(file, secret);

        #region Cryptography

        public string Encrypt(string Secret) => this.EncryptToRow(Secret);
        public License Decrypt(string row, string Secret) => row.DecryptRow<License>(Secret);

        #endregion

        #region Validation and Checks

        /// <summary>
        /// Validate license for this pc by hdd 'C'
        /// </summary>
        /// <returns>validation result</returns>
        public bool ValidateLicense() => License.ValidateLicense(this, GetThisPcHddSerialNumber());

        /// <summary>
        /// Validate license
        /// </summary>
        /// <param name="license">license data</param>
        /// <param name="HDDid">hdd id</param>
        /// <returns>validation result</returns>
        public static bool ValidateLicense([NotNull] License license, string HDDid)
        {
            if (license.HDDid.IsNotNullOrWhiteSpace() && HDDid != license.HDDid)
            {
                return false;
            }

            return license.ExpirationDate is null || DateTime.Now <= license.ExpirationDate;
        }

        #endregion

        /// <summary>
        /// Get string license information
        /// </summary>
        /// <returns></returns>
        [NotNull]
        public string GetLicenseInformation()
        {
            return (ExpirationDate is not null) switch
            {
                true when HDDid.IsNotNullOrWhiteSpace() => $"License for {IssuedFor}, HDD: {HDDid}, expires {ExpirationDate:dd.MM.yyyy HH:mm}",
                false when HDDid.IsNullOrWhiteSpace() => $"License for {IssuedFor}, UNLIMITED license",
                false => $"UNLIMITED license for {IssuedFor}, PC with HDD: {HDDid}",
                _ => $"license expires {ExpirationDate:dd.MM.yyyy HH:mm:ss} for {IssuedFor}, any PC"
            };
        }
        public static string GetThisPcHddSerialNumber(char hdd_char_name = 'c') => HDDInfo.GetSerialNumber($"{hdd_char_name}:\\").ToString("X");
        [NotNull] public override string ToString() => GetLicenseInformation();

        public static readonly Regex HDDidRegex = new Regex(@"[0-9a-hA-h]*", RegexOptions.Compiled);

    }

    public static class LicenseExtensions
    {
        /// <summary>
        /// Save data to the file
        /// </summary>
        /// <param name="lic">license file</param>
        /// <param name="FilePath">path where file will be save</param>
        /// <param name="secret">secret row to cover license</param>
        /// <returns>path where file was saved</returns>
        public static string SaveToFile(
            [NotNull] this License lic,
            string FilePath,
            string secret) =>
            lic.SaveToZipFile(
                FilePath,
                $"{License.LicenseDirectory}\\{License.LicenseFileName}",
                secret);

        /// <summary>
        /// Save data to the file
        /// </summary>
        /// <param name="lic">license file</param>
        /// <param name="FilePath">path where file will be save</param>
        /// <param name="secret">secret row to cover license</param>
        /// <returns>path where file was saved</returns>
        public static async Task<string> SaveToFileAsync(
            [NotNull] this License lic,
            [NotNull] string FilePath,
            string secret) =>
            await lic.SaveToZipFileAsync(
                FilePath,
                $"{License.LicenseDirectory}\\{License.LicenseFileName}",
                secret);

        /// <summary>
        /// Load data from the file
        /// </summary>
        /// <param name="license">license file</param>
        /// <param name="file">file with license</param>
        /// <param name="secret">secret row for discover license</param>
        public static void LoadFromFile([NotNull] this License license, [NotNull] FileInfo file, [NotNull] string secret)
        {
            try
            {
                if (!file.Exists)
                    throw new FileNotFoundException(file.FullName, "License file not found");

                //if (secret is null)
                //    throw new ArgumentNullException(nameof(secret), "Secret row can't be null");

                var lic = file.GetFromZip<License>(License.LicenseFileName, secret);
                if (lic is null)
                    throw new LicenseExceptions("License not fount", nameof(LoadFromFileAsync));
                license.HDDid = lic.HDDid;
                license.ExpirationDate = lic.ExpirationDate;
                license.CheckSessions = lic.CheckSessions;
                license.IssuedFor = lic.IssuedFor;

                if (lic.ExpirationDate is { } date && date < DateTime.Now)
                    throw new LicenseExceptions("License is expired", nameof(LoadFromFileAsync));
            }
            catch (FormatException e)
            {
                throw new LicenseExceptions("Invalid format string", nameof(LoadFromFileAsync), e);
            }
            catch (CryptographicException e)
            {
                throw new LicenseExceptions("Invalid cover string", nameof(LoadFromFileAsync), e);
            }
        }
        /// <summary>
        /// Load data from the file
        /// </summary>
        /// <param name="license">license file</param>
        /// <param name="file">file with license</param>
        /// <param name="secret">secret row for discover license</param>
        public static async Task LoadFromFileAsync([NotNull] this License license, [NotNull] FileInfo file, [NotNull] string secret)
        {
            try
            {
                if (!file.Exists)
                    throw new FileNotFoundException(file.FullName, "License file not found");

                //if (secret is null)
                //    throw new ArgumentNullException(nameof(secret), "Secret row can't be null");

                var lic = await file.GetFromZipAsync<License>(License.LicenseFileName, secret);
                if(lic is null)
                    throw new LicenseExceptions("License not fount", nameof(LoadFromFileAsync));
                license.HDDid = lic.HDDid;
                license.ExpirationDate = lic.ExpirationDate;
                license.CheckSessions = lic.CheckSessions;
                license.IssuedFor = lic.IssuedFor;
            }
            catch (FormatException e)
            {
                throw new LicenseExceptions("Invalid format string", nameof(LoadFromFileAsync), e);
            }
            catch (CryptographicException e)
            {
                throw new LicenseExceptions("Invalid cover string", nameof(LoadFromFileAsync), e);
            }

        }

    }
}
