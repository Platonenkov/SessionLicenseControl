using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MathCore.Annotations;
using SessionLicenseControl.Extensions;
using SessionLicenseControl.Interfaces;
using SessionLicenseControl.Licenses;
using SessionLicenseControl.Sessions;

namespace SessionLicenseControl
{
    public class LicenseGenerator : ILicenseBase
    {
        public readonly FileInfo LicenseFile;

        #region For License Properties

        #region property HDDid:string

        private static readonly Regex sf_HDDidRegex = new(@"[0-9a-hA-h]*", RegexOptions.Compiled);

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

        public bool CheckSessions { get; set; }
        public string IssuedFor { get; set; }

        #endregion

        #endregion

        /// <summary> secret string to encrypt data </summary>
        public string Secret { get; set; }

        #region Constructors

        public LicenseGenerator()
        {

        }
        public LicenseGenerator(FileInfo file, [CanBeNull] string HDD, [CanBeNull] DateTime? expirationDate, bool check_sessions,string issued_for, [NotNull] string secret)
        {
            IssuedFor = issued_for;
            LicenseFile = file;
            HDDid = HDD;
            ExpirationDate = expirationDate;
            Secret = secret;
            CheckSessions = check_sessions;
        }
        public LicenseGenerator([NotNull] string secret, [CanBeNull] string HDD, [CanBeNull] DateTime? expirationDate, string issued_for, bool check_sessions)
        {
            IssuedFor = issued_for;
            HDDid = HDD;
            Secret = secret;
            ExpirationDate = expirationDate;
            CheckSessions = check_sessions;
        }

        #endregion

        #region License

        /// <summary> Generate license text </summary>
        /// <returns>license code</returns>
        public string GetLicenseEncryptedRow() => GetLicense().Encrypt(Secret);

        /// <summary> Generate license </summary>
        /// <returns>license</returns>
        [NotNull]
        public License GetLicense() => new(HDDid, ExpirationDate, IssuedFor,CheckSessions );

        /// <summary> Create license file </summary>
        /// <param name="FullFileName">full file name to license</param>
        /// <returns>full path to license file</returns>
        [NotNull]
        public string CreateLicenseFile(string FullFileName)
            => SaveData(FullFileName, Secret, GetLicense());

        /// <summary> Create license file </summary>
        /// <returns>full path to license file</returns>
        [NotNull]
        public string CreateLicenseFile() => CreateLicenseFile(LicenseFile.FullName);

        #region Save

        /// <summary> Save data to the file </summary>
        [NotNull]
        public static string SaveData([NotNull] string FilePath, string secret, [NotNull] License lic) => SaveDataAsync(FilePath, secret, lic).Result;
        /// <summary> Save data to the file </summary>
        /// <exception cref="ArgumentNullException">if file path is null</exception>
        /// <param name="FilePath">path to license file</param>
        /// <param name="secret">secret string if you want to encrypt data</param>
        /// <param name="lic">license</param>
        /// <returns></returns>
        [ItemNotNull]
        public static async Task<string> SaveDataAsync([NotNull] string FilePath, string secret, [NotNull] License lic)
        {
            if (FilePath.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(FilePath));

            await lic.SaveToZipFileAsync(FilePath, $"{License.LicenseDirectory}\\{License.LicenseFileName}", secret);
            if (lic.CheckSessions)
            {
                var session = new WorkDay(DateTime.Now, null) { LastSession = { Information = "License created" } };
                session.LastSession.CloseSession();
                await session.SaveToZipFileAsync(FilePath, SessionsOperator.GetCurrentDayFileName(), secret);
            }
            return FilePath;
        }

        #endregion

        #endregion

    }

}
