using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MathCore.Annotations;
using SessionLicenseControl.Exceptions;
using SessionLicenseControl.Information;

namespace SessionLicenseControl.Licenses
{
    public class LicenseController
    {
        public readonly FileInfo LicenseFile;

        #region For License Properties

        #region property EnableHDD:bool 

        /// <summary>Enable check by HDD id</summary>
        public bool EnableHDD { get; set; }

        #endregion

        #region property HDDid:string

        private static readonly Regex sf_HDDidRegex = new Regex(@"[0-9a-hA-h]*", RegexOptions.Compiled);

        /// <summary>HDD id</summary>
        private string f_HDDid;

        /// <summary>HDD id</summary>
        public string HDDid
        {
            get => f_HDDid;
            set
            {
                if (Equals(f_HDDid, value)) return;
                if (value != null && !sf_HDDidRegex.IsMatch(value))
                    throw new FormatException("HDD id invalid format");
                f_HDDid = value;
                EnableHDD = value is not null;
            }
        }

        #endregion

        #region Enable Date for license

        /// <summary>Enable check by date</summary>
        public bool EnableDate { get; set; }

        private DateTime? _ExpirationDate;

        /// <summary>Expiration date</summary>
        public DateTime? ExpirationDate
        {
            get => _ExpirationDate;
            set
            {
                _ExpirationDate = value;
                EnableDate = value is not null;
            }
        }

        #endregion

        #endregion

        /// <summary> Secret row for cover license code </summary>
        public string Secret { get; set; }

        #region Constructors

        public LicenseController(FileInfo file, string secret)
        {
            LicenseFile = file;
            Secret = secret;
            var lic = LoadData(LicenseFile.FullName, secret);
            if (lic.HDD.IsNotNullOrWhiteSpace())
                HDDid = lic.HDD;
            if (lic.Date is { } date)
                ExpirationDate = date.Date;
        }
        public LicenseController(FileInfo file, string HDD, DateTime? date, string secret)
        {
            LicenseFile = file;
            HDDid = HDD;
            ExpirationDate = date;
            Secret = secret;
        }
        public LicenseController(FileInfo file, DateTime date, string secret)
        {
            LicenseFile = file;
            ExpirationDate = date;
            Secret = secret;
        }
        public LicenseController(FileInfo file, string HDD, string secret)
        {
            LicenseFile = file;
            HDDid = HDD;
            Secret = secret;
        }
        public LicenseController(string secret, string HDD, DateTime date)
        {
            HDDid = HDD;
            Secret = secret;
            ExpirationDate = date;
        }
        public LicenseController(string secret, string HDD)
        {
            HDDid = HDD;
            Secret = secret;
        }
        public LicenseController(string secret, DateTime date)
        {
            Secret = secret;
            ExpirationDate = date;
        }

        #endregion

        #region License

        #region Overrides of Object

        [NotNull] public override string ToString() => GetLicenseInformation();

        #endregion

        /// <summary>
        /// Get string license information
        /// </summary>
        /// <returns></returns>
        [NotNull]
        public string GetLicenseInformation()
        {
            switch (EnableDate)
            {
                case true when EnableHDD:
                    return $"License for HDD: {HDDid}, expires {ExpirationDate}";
                case false when !EnableHDD:
                    return "UNLIMITED license";
                case false:
                    return $"UNLIMITED license for PC with HDD: {HDDid}";
                default:
                    return $"license expires {ExpirationDate:dd.MM.yyyy HH:mm} for any PC";
            }
        }
        /// <summary> Set HDD for this PC </summary>
        public void SetForThisPC() => HDDid = License.GetThisPcHddSerialNumber();
        /// <summary> Generate license text </summary>
        /// <returns>license code</returns>
        public string GetLicenseCodeRow() => GetLicense().Encrypt(Secret);
        /// <summary> Generate license </summary>
        /// <returns>license</returns>
        [NotNull]
        public License GetLicense()
        {
            var license = new License();
            if (EnableHDD)
                license.HDD = HDDid;
            if (EnableDate)
                license.Date = ExpirationDate;
            return license;
        }

        /// <summary> Create license file </summary>
        /// <returns>true result or error</returns>
        public string CreateLicenseFile(string FileName) => SaveData(FileName, Secret, GetLicense());

        /// <summary> Create license file </summary>
        /// <returns>true result or error</returns>
        public string CreateLicenseFile() => SaveData(LicenseFile.FullName, Secret, GetLicense());

        #region Save/Load

        /// <summary> Load data from the file </summary>
        public static async Task<License> LoadDataAsync(string FilePath, string secret)
        {
            try
            {
                var lic = new License();
                await lic.LoadFromFileAsync(new FileInfo(FilePath), secret);
                return lic;
            }
            catch (FormatException e)
            {
                throw new LicenseExceptions("Invalid format string", nameof(LoadData), e);
            }
            catch (CryptographicException e)
            {
                throw new LicenseExceptions("Invalid cover string", nameof(LoadData), e);
            }
        }

        /// <summary> Load data from the file </summary>
        public static License LoadData(string FilePath, string secret) => LoadDataAsync(FilePath, secret).Result;

        /// <summary> Save data to the file </summary>
        public static string SaveData(string FilePath, string secret, License lic) => SaveDataAsync(FilePath, secret, lic).Result;
        /// <summary> Save data to the file </summary>
        public static async Task<string> SaveDataAsync(string FilePath, string secret, License lic)
        {
            await lic.SaveToFileAsync(FilePath, secret);
            var data = lic.Encrypt(true, secret);

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

        #endregion

        #region Validate License

        public static bool CheckLicense(string row, string secret)
        {
            try
            {
                return new License(row,secret).IsValid;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool CheckLicense() => CheckLicense(LicenseFile, Secret);
        public static bool CheckLicense([NotNull] FileInfo file, string secret)
        {
            try
            {
                return new License(file, secret).IsValid;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public static async Task<bool> CheckLicenseAsync([NotNull] FileInfo FilePath, string secret)
        {
            try
            {
                return (await LoadDataAsync(FilePath.FullName, secret)).IsValid;
            }
            catch (Exception)
            {
                return false;
            }

        }
        #endregion
        #endregion

    }
}