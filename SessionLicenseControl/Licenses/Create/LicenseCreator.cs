//using System;
//using System.IO;
//using System.Text;
//using System.Text.RegularExpressions;
//using System.Threading.Tasks;
//using MathCore.Annotations;

//namespace SessionLicenseControl.Licenses.Create
//{
//    public class LicenseCreator
//    {
//        public readonly FileInfo LicenseFile;

//        #region For License Properties

//        #region property EnableHDD:bool 

//        /// <summary>Enable check by HDD id</summary>
//        public bool EnableHDD { get; set; }

//        #endregion

//        #region property HDDid:string

//        private static readonly Regex sf_HDDidRegex = new Regex(@"[0-9a-hA-h]*", RegexOptions.Compiled);

//        /// <summary>HDD id</summary>
//        private string f_HDDid;

//        /// <summary>HDD id</summary>
//        public string HDDid
//        {
//            get => f_HDDid;
//            set
//            {
//                if (Equals(f_HDDid, value)) return;
//                if (value != null && !sf_HDDidRegex.IsMatch(value))
//                    throw new FormatException("HDD id invalid format");
//                f_HDDid = value;
//                EnableHDD = value is not null;
//            }
//        }

//        #endregion

//        #region Enable Date for license

//        /// <summary>Enable check by date</summary>
//        public bool EnableDate { get; set; }

//        private DateTime? _ExpirationDate;

//        /// <summary>Expiration date</summary>
//        public DateTime? ExpirationDate
//        {
//            get => _ExpirationDate;
//            set
//            {
//                _ExpirationDate = value;
//                EnableDate = value is not null;
//            }
//        }

//        #endregion

//        #endregion

//        /// <summary> Secret row for cover license code </summary>
//        public string Secret { get; set; }

//        #region Constructors

//        public LicenseCreator()
//        {

//        }
//        public LicenseCreator(FileInfo file, string HDD, DateTime date, string secret)
//        {
//            LicenseFile = file;
//            HDDid = HDD;
//            ExpirationDate = date;
//            Secret = secret;
//        }
//        public LicenseCreator(FileInfo file, DateTime date, string secret)
//        {
//            LicenseFile = file;
//            ExpirationDate = date;
//            Secret = secret;
//        }
//        public LicenseCreator(FileInfo file, string HDD, string secret)
//        {
//            LicenseFile = file;
//            HDDid = HDD;
//            Secret = secret;
//        }
//        public LicenseCreator(string secret, string HDD, DateTime date)
//        {
//            HDDid = HDD;
//            Secret = secret;
//            ExpirationDate = date;
//        }
//        public LicenseCreator(string secret, string HDD)
//        {
//            HDDid = HDD;
//            Secret = secret;
//        }
//        public LicenseCreator(string secret, DateTime date)
//        {
//            Secret = secret;
//            ExpirationDate = date;
//        }

//        #endregion

//        #region License

//        /// <summary> Set HDD for this PC </summary>
//        public void SetForThisPC() => HDDid = License.GetThisPcHddSerialNumber();

//        /// <summary> Generate license text </summary>
//        /// <returns>license code</returns>
//        public string GetLicenseCodeRow() => GetLicense().Encrypt(Secret);
//        /// <summary> Generate license </summary>
//        /// <returns>license</returns>
//        [NotNull]
//        public License GetLicense()
//        {
//            var license = new License();
//            if (EnableHDD)
//                license.HDD = HDDid;
//            if (EnableDate)
//                license.Date = ExpirationDate;
//            return license;
//        }

//        /// <summary> Create license file </summary>
//        /// <returns>true result or error</returns>
//        public string CreateLicenseFile(string FileName) => SaveData(FileName, Secret, GetLicense());

//        /// <summary> Create license file </summary>
//        /// <returns>true result or error</returns>
//        public string CreateLicenseFile() => SaveData(LicenseFile.FullName, Secret, GetLicense());

//        #region Save/Load

//        /// <summary> Save data to the file </summary>
//        public static string SaveData(string FilePath, string secret, License lic) => SaveDataAsync(FilePath, secret, lic).Result;
//        /// <summary> Save data to the file </summary>
//        public static async Task<string> SaveDataAsync(string FilePath, string secret, License lic)
//        {
//            var data = lic.Encrypt(true, secret);

//            var file = new FileInfo(FilePath);
//            file.CreateParentIfNotExist();
//            var time_out_count = 0;
//            while (file.IsLocked() && time_out_count < 100)
//            {
//                await Task.Delay(300);
//                time_out_count++;
//            }

//            await File.WriteAllTextAsync(FilePath, data, Encoding.UTF8);
//            return FilePath;
//        }

//        #endregion

//        #endregion

//    }
//}