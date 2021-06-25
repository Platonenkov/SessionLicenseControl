using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
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

        public License()
        {

        }

        public License(string row, string secret)
        {
            var input = Decrypt(row, secret);
            this.Date = input.Date;
            this.HDD = input.HDD;
        }

        public License(FileInfo file, string secret) => this.LoadFromFile(file, secret);

        public string Encrypt(string Secret) => this.CreateDataRow(true, Secret);
        public static License Decrypt(string row, string Secret)
        {
            try
            {
                return row.GetDataFromRow<License>(true, Secret);
            }
            catch (FormatException e)
            {
                throw new LicenseExceptions("Invalid License string", nameof(Decrypt), e);
            }
            catch (CryptographicException e)
            {
                throw new LicenseExceptions("Invalid cover string", nameof(Decrypt), e);
            }
        }
        /// <summary>
        /// Validate license for this pc by hdd 'C'
        /// </summary>
        /// <returns>validation result</returns>
        internal bool ValidateLicenseForThisPC() => this.ValidateLicense(LicenseController.GetThisPcHDD());

        /// <summary> Save data to the file </summary>
        public bool SaveToFile(string FilePath, string secret) => SaveAsync(FilePath, secret).Result;

        /// <summary> Save data to the file </summary>
        public async Task<bool> SaveAsync(string FilePath, string secret) => await this.SaveToFileAsync(FilePath, secret);

    }

    public static class LicenseExtensions
    {
        /// <summary>
        /// Validate license
        /// </summary>
        /// <param name="license">license data</param>
        /// <param name="HDDid">hdd id</param>
        /// <returns>validation result</returns>
        internal static bool ValidateLicense([NotNull] this License license, string HDDid)
        {
            if (license.HDD.IsNotNullOrWhiteSpace() && HDDid != license.HDD)
            {
                return false;
            }

            return license.Date is null || DateTime.Now <= license.Date;
        }
        /// <summary> Save data to the file </summary>
        public static bool SaveToFile([NotNull] this License lic, string FilePath, string secret) => lic.SaveToFileAsync(FilePath, secret).Result;
        /// <summary> Save data to the file </summary>
        public static async Task<bool> SaveToFileAsync([NotNull] this License lic, [NotNull] string FilePath, string secret)
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
            return true;
        }
        /// <summary> Load data from the file </summary>
        public static void LoadFromFile([NotNull] this License license, [NotNull] FileInfo file, [NotNull] string secret)
            => license.LoadFromFileAsync(file, secret).Wait();
        /// <summary> Load data from the file </summary>
        public static async Task LoadFromFileAsync([NotNull] this License license, [NotNull] FileInfo file, [NotNull] string secret)
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
            var lic = lic_text.GetDataFromRow<License>(true, secret);
            license.HDD = lic.HDD;
            license.Date = lic.Date;
        }

    }
}
