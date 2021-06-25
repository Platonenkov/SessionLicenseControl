using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MathCore.Annotations;
using SessionLicenseControl.Interfaces;
using SessionLicenseControl.Licenses;

namespace SessionLicenseControl
{
    public class LicenseGenerator : ILicenseBase
    {
        public readonly FileInfo LicenseFile;

        #region For License Properties

        #region property HDDid:string

        private static readonly Regex sf_HDDidRegex = new (@"[0-9a-hA-h]*", RegexOptions.Compiled);

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
            }
        }

        #endregion

        #region Enable Date for license


        /// <summary>Expiration date</summary>
        public DateTime? ExpirationDate { get; set; }
        #endregion

        #endregion

        /// <summary> Secret row for cover license code </summary>
        public string Secret { get; set; }

        #region Constructors

        public LicenseGenerator()
        {

        }
        public LicenseGenerator(FileInfo file, [CanBeNull] string HDD, [CanBeNull] DateTime? date, [NotNull] string secret)
        {
            LicenseFile = file;
            HDDid = HDD;
            ExpirationDate = date;
            Secret = secret;
        }
        public LicenseGenerator([NotNull] string secret, [CanBeNull] string HDD, [CanBeNull] DateTime? date)
        {
            HDDid = HDD;
            Secret = secret;
            ExpirationDate = date;
        }

        #endregion

        #region License

        /// <summary> Set HDD for this PC </summary>
        public void SetForThisPC() => HDDid = License.GetThisPcHddSerialNumber();

        /// <summary> Generate license text </summary>
        /// <param name="WithSessionDataControl">add session data to license control</param>
        /// <returns>license code</returns>
        public string GetLicenseCoveredRow(bool WithSessionDataControl) 
            => WithSessionDataControl
                ? GetLicenseWithSession().Encrypt(Secret)
                : GetLicense().Encrypt(Secret);

        /// <summary> Generate license </summary>
        /// <returns>license</returns>
        [NotNull]
        public License GetLicense() => new(HDDid, ExpirationDate);
        /// <summary> Generate license </summary>
        /// <returns>license</returns>
        [NotNull]
        public LicenseWithSessions GetLicenseWithSession() => new(HDDid, ExpirationDate);

        /// <summary> Create license file </summary>
        /// <param name="FullFileName">full file name to license</param>
        /// <param name="WithSessionDataControl">add session data to license control</param>
        /// <returns>full path to license file</returns>
        public string CreateLicenseFile(string FullFileName, bool WithSessionDataControl) 
            => WithSessionDataControl
                ? SaveData(FullFileName, Secret, GetLicenseWithSession())
                : SaveData(FullFileName, Secret, GetLicense());

        /// <summary> Create license file </summary>
        /// <param name="WithSessionDataControl">add session data to license control</param>
        /// <returns>full path to license file</returns>
        public string CreateLicenseFile(bool WithSessionDataControl) => CreateLicenseFile(LicenseFile.FullName, WithSessionDataControl);

        #region Save

        /// <summary> Save data to the file </summary>
        public static string SaveData<T>(string FilePath, string secret, T lic) => SaveDataAsync(FilePath, secret, lic).Result;
        /// <summary> Save data to the file </summary>
        /// <exception cref="ArgumentNullException">if file path is null</exception>
        /// <typeparam name="T">License type</typeparam>
        /// <param name="FilePath">path to license file</param>
        /// <param name="secret">secret row to cover data</param>
        /// <param name="lic">license</param>
        /// <returns></returns>
        public static async Task<string> SaveDataAsync<T>([NotNull] string FilePath, string secret, T lic)
        {
            if (FilePath.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(FilePath));
            var data = lic.EncryptToRow(true, secret);

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

        #endregion

    }

}
