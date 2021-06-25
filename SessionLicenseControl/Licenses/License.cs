using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MathCore.Annotations;
using SessionLicenseControl.Exceptions;
using SessionLicenseControl.Information;

namespace SessionLicenseControl.Licenses
{
    public class License
    {
        public string HDD { get; set; }
        public DateTime? Date { get; set; }
        public bool IsValid => ValidateLicenseForThisPC();

        public License()
        {

        }
        public License(string hdd, DateTime? ExpirationDate)
        {
            HDD = hdd;
            Date = ExpirationDate;
        }

        public License(string row, string secret)
        {
            var input = Decrypt(row, secret);
            Date = input.Date;
            HDD = input.HDD;
        }

        public License(FileInfo file, string secret) => this.LoadFromFile(file, secret);

        #region Cryptography

        internal string Encrypt(string Secret) => this.Encrypt(true, Secret);
        public static License Decrypt(string row, string Secret) => row.Decrypt<License>(true, Secret);

        #endregion

        #region Validation and Checks

        /// <summary>
        /// Validate license for this pc by hdd 'C'
        /// </summary>
        /// <returns>validation result</returns>
        private bool ValidateLicenseForThisPC() => License.ValidateLicense(this, GetThisPcHddSerialNumber());

        /// <summary>
        /// Validate license
        /// </summary>
        /// <param name="license">license data</param>
        /// <param name="HDDid">hdd id</param>
        /// <returns>validation result</returns>
        public static bool ValidateLicense([NotNull] License license, string HDDid)
        {
            if (license.HDD.IsNotNullOrWhiteSpace() && HDDid != license.HDD)
            {
                return false;
            }

            return license.Date is null || DateTime.Now <= license.Date;
        }

        #endregion
        /// <summary>
        /// Get string license information
        /// </summary>
        /// <returns></returns>
        [NotNull]
        public string GetLicenseInformation()
        {
            return (Date is not null) switch
            {
                true when HDD.IsNotNullOrWhiteSpace() => $"License for HDD: {HDD}, expires {Date:dd.MM.yyyy HH:mm}",
                false when HDD.IsNullOrWhiteSpace() => "UNLIMITED license",
                false => $"UNLIMITED license for PC with HDD: {HDD}",
                _ => $"license expires {Date:dd.MM.yyyy HH:mm} for any PC"
            };
        }
        public static string GetThisPcHddSerialNumber(char hdd_char_name = 'c') => HDDInfo.GetSerialNumber($"{hdd_char_name}:\\").ToString("X");
        [NotNull] public override string ToString() => GetLicenseInformation();

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
        public static string SaveToFile([NotNull] this License lic, string FilePath, string secret) => lic.SaveToFileAsync(FilePath, secret).Result;

        /// <summary>
        /// Save data to the file
        /// </summary>
        /// <param name="lic">license file</param>
        /// <param name="FilePath">path where file will be save</param>
        /// <param name="secret">secret row to cover license</param>
        /// <returns>path where file was saved</returns>
        public static async Task<string> SaveToFileAsync([NotNull] this License lic, [NotNull] string FilePath, string secret)
        {
            var data = lic.Encrypt(secret);

            var file = new FileInfo(FilePath);
            file.CreateParentIfNotExist();
            var time_out_count = 0;
            while (file.IsLocked() && time_out_count < 100)
            {
                await Task.Delay(300);
                time_out_count++;
            }

            await File.WriteAllTextAsync(FilePath, data, Encoding.UTF8);
            return FilePath;
        }
        /// <summary>
        /// Load data from the file
        /// </summary>
        /// <param name="license">license file</param>
        /// <param name="file">file with license</param>
        /// <param name="secret">secret row for discover license</param>
        public static void LoadFromFile([NotNull] this License license, [NotNull] FileInfo file, [NotNull] string secret)
            => license.LoadFromFileAsync(file, secret).Wait();
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

                if (secret is null)
                    throw new ArgumentNullException(nameof(secret), "Secret row can't be null");

                var time_out_count = 0;
                while (file.IsLocked() && time_out_count < 100)
                {
                    await Task.Delay(300);
                    time_out_count++;
                }

                var lic_text = await File.ReadAllTextAsync(file.FullName, Encoding.UTF8);
                var lic = lic_text.Decrypt<License>(true, secret);
                license.HDD = lic.HDD;
                license.Date = lic.Date;
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
