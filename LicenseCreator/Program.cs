using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Input;
using ConsoleEx;
using SessionLicenseControl.Information;

namespace LicenseCreator
{
    class Program
    {
        private static string LicenseFileName = "License.lic";
        static void Main(string[] args)
        {
            while (true)
            {
                var license = new LicenseController(LicenseFileName)
                {
                    SelectedDate = DateTime.Now.AddDays(300),
                    SelectedDateChecked = true,
                    HDDidChecked = true
                };
                Console.WriteLine(license.LicenseText);
                Console.ReadLine();
            }

        }
    }
    internal class LicenseController
    {
        private readonly string _FileName;

        #region property HDDidChecked:bool // Выбран метод проверки по id HDD

        /// <summary>Выбран метод проверки по id HDD</summary>
        public bool HDDidChecked { get; set; }

        #endregion

        #region property HDDid:string // Идентификатор HDD

        private static readonly Regex sf_HDDidRegex = new Regex(@"[0-9a-hA-h]*", RegexOptions.Compiled);

        /// <summary>Идентификатор HDD</summary>
        private string f_HDDid;

        /// <summary>Идентификатор HDD</summary>
        public string HDDid
        {
            get => f_HDDid;
            set
            {
                if (Equals(f_HDDid, value)) return;
                if (value != null && !sf_HDDidRegex.IsMatch(value))
                    throw new FormatException("Строка идентификатора HDD имела неправильный формат");
                f_HDDid = value;
                UpdateLicense();
            }
        }

        #endregion

        /// <summary>Выбран метод проверки по времени</summary>
        public bool SelectedDateChecked { get; set; }

        private DateTime _SelectedDate = DateTime.Now.AddDays(10);

        /// <summary>Время ограничения</summary>
        public DateTime SelectedDate
        {
            get => _SelectedDate;
            set
            {
                _SelectedDate = value;
                UpdateLicense();
            }
        }

        /// <summary>Текст лицензии</summary>
        public string LicenseText { get; set; }

        /// <summary> Строка шифрования </summary>
        public string CoverRow { get; set; };
        public LicenseController(string fileName)
        {
            _FileName = fileName;
            HDDid = HDDInfo.GetSerialNimber("c:\\").ToString("X");

            UpdateLicense();
        }

        private void UpdateLicense()
        {
            var license_builder = new StringBuilder("{");
            if (HDDidChecked) license_builder.Append($"\"hdd\"=\"{f_HDDid}\"");
            if (SelectedDateChecked)
            {
                if (HDDidChecked) license_builder.Append(",");
                license_builder.Append($"\"date\"=\"{SelectedDate.ToShortDateString()}\"");
            }
            license_builder.Append("}");
            var license_string = license_builder.ToString();
            LicenseText = license_string.Cover(CoverRow);
        }

        private bool CreateLicense()
        {
            if (CanCreateLicanse())
            {
                File.WriteAllText(_FileName, LicenseText, Encoding.UTF8);
                return true;
            }

            "Select options".ConsoleRed();
            return false;
        }

        private bool CanCreateLicanse() => SelectedDateChecked || HDDidChecked;
    }

}
