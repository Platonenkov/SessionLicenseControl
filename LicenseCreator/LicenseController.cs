using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using SessionLicenseControl.Information;
using SessionLicenseControl.Licenses;

namespace LicenseCreator
{
    internal class LicenseController
    {
        public readonly string FileName;

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
                EnableHDD = true;
            }
        }

        #endregion

        #region Enable Date for license

        /// <summary>Enable check by date</summary>
        public bool EnableDate { get; set; }

        private DateTime _ExpirationDate;

        /// <summary>Expiration date</summary>
        public DateTime ExpirationDate
        {
            get => _ExpirationDate;
            set
            {
                _ExpirationDate = value;
                EnableDate = true;
            }
        }

        #endregion

        /// <summary> Secret row for cover license code </summary>
        public string Secret { get; set; }

        #region Constructors

        public LicenseController(string fileName)
        {
            FileName = fileName;
        }
        public LicenseController(string fileName, string HDD, DateTime date, string secret)
        {
            FileName = fileName;
            HDDid = HDD;
            ExpirationDate = date;
            Secret = secret;
        } 
        public LicenseController(string fileName, DateTime date, string secret)
        {
            FileName = fileName;
            ExpirationDate = date;
            Secret = secret;
        } 
        public LicenseController(string fileName, string HDD, string secret)
        {
            FileName = fileName;
            HDDid = HDD;
            Secret = secret;
        }

        #endregion
        /// <summary> Set HDD for this PC </summary>
        public void GetForThisPC() => HDDid = HDDInfo.GetSerialNimber("c:\\").ToString("X");

        /// <summary> Generate license text </summary>
        /// <returns>license code</returns>
        public string GetLicenseCodeRow()
        {
            var license = new License();
            if (EnableHDD)
                license.HDD = HDDid;
            if (EnableDate)
                license.Date = ExpirationDate;
            return license.CreateDataRow(true, Secret);
        }
        /// <summary> Create license file </summary>
        /// <returns>true result or error</returns>
        public bool CreateLicenseFile()
        {
            File.WriteAllText(FileName, GetLicenseCodeRow(), Encoding.UTF8);
            return true;
        }

        private bool CanCreateLicense() => EnableHDD || EnableDate;
    }
}