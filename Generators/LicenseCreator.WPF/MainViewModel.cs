using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;
using Microsoft.Win32;
using SessionLicenseControl;
using SessionLicenseControl.Interfaces;
using SessionLicenseControl.WPF.Commands;
using License = SessionLicenseControl.Licenses.License;

namespace LicenseCreator.WPF
{
    internal class MainViewModel : MathCore.ViewModels.ViewModel, ILicenseBase
    {
        #region Secret : string - строка шифрования

        /// <summary>строка шифрования</summary>
        private string _Secret = "";

        /// <summary>строка шифрования</summary>
        public string Secret { get => _Secret; set => Set(ref _Secret, value); }

        #endregion
        #region property HDDidChecked:bool // Выбран метод проверки по id HDD

        /// <summary>Выбран метод проверки по id HDD</summary>
        private bool f_HDDidChecked;

        /// <summary>Выбран метод проверки по id HDD</summary>
        public bool HDDidChecked
        {
            get => f_HDDidChecked;
            set
            {
                if (Equals(f_HDDidChecked, value)) return;
                f_HDDidChecked = value;
                OnPropertyChanged(nameof(HDDidChecked));
            }
        }

        #endregion

        #region property HDDid:string // Идентификатор HDD

        /// <summary>Идентификатор HDD</summary>
        private string f_HDDid = License.GetThisPcHddSerialNumber();

        /// <summary>Идентификатор HDD</summary>
        public string HDDid
        {
            get => f_HDDid;
            set
            {
                if (Equals(f_HDDid, value)) return;
                if (value != null && !License.HDDidRegex.IsMatch(value))
                    throw new FormatException("Строка идентификатора HDD имела неправильный формат");
                f_HDDid = value;
                OnPropertyChanged(nameof(HDDid));
            }
        }

        #region CheckSessions : bool - Проверка сессий в лицензии

        /// <summary>Проверка сессий в лицензии</summary>
        private bool _CheckSessions;

        /// <summary>Проверка сессий в лицензии</summary>
        public bool CheckSessions { get => _CheckSessions; set => Set(ref _CheckSessions, value); }

        #endregion

        #region IssuedFor : string - License owner

        /// <summary>License owner</summary>
        private string _IssuedFor = License.DefaultIssuedFor;

        /// <summary>License owner</summary>
        public string IssuedFor { get => _IssuedFor; set => Set(ref _IssuedFor, value); }

        #endregion

        #endregion

        #region property DatePeriodSelected:bool // Выбран метод проверки по времени

        /// <summary>Выбран метод проверки по времени</summary>
        private bool f_SelectedDateChecked;

        /// <summary>Выбран метод проверки по времени</summary>
        public bool SelectedDateChecked
        {
            get => f_SelectedDateChecked;
            set
            {
                if (Equals(f_SelectedDateChecked, value)) return;
                f_SelectedDateChecked = value;
                OnPropertyChanged(nameof(SelectedDateChecked));
            }
        }

        #endregion

        #region property SelectedDate:DateTime // Время ограничения

        /// <summary>Время ограничения</summary>
        private DateTime? f_SelectedDate = DateTime.Now.AddDays(10);

        /// <summary>Время ограничения</summary>
        public DateTime? ExpirationDate
        {
            get => f_SelectedDate;
            set
            {
                if (Equals(f_SelectedDate, value)) return;
                f_SelectedDate = value;
                OnPropertyChanged(nameof(ExpirationDate));
            }
        }

        #endregion

        #region property LicenseText:string // Текст лицензии

        /// <summary>Текст лицензии</summary>
        private string f_LicenseText;

        /// <summary>Текст лицензии</summary>
        public string LicenseText
        {
            get => f_LicenseText;
            private set
            {
                if (Equals(f_LicenseText, value)) return;
                f_LicenseText = value;
                OnPropertyChanged(nameof(LicenseText));
            }
        }

        #endregion

        [NotNull] public ICommand CreateLicenseCommand { get; }

        [NotNull] public ICommand GetCurrentHDDid { get; }

        public MainViewModel()
        {
            CreateLicenseCommand = new LamdaCommand(_ => CreateLicense());
            GetCurrentHDDid = new LamdaCommand(_ => HDDid = License.GetThisPcHddSerialNumber());
            void Updater(object s, PropertyChangedEventArgs e) => UpdateLicense();
            this.SubscribeTo(nameof(HDDid), Updater);
            this.SubscribeTo(nameof(HDDidChecked), Updater);
            this.SubscribeTo(nameof(ExpirationDate), Updater);
            this.SubscribeTo(nameof(SelectedDateChecked), Updater);
            this.SubscribeTo(nameof(Secret), Updater);
            this.SubscribeTo(nameof(IssuedFor), Updater);
            this.SubscribeTo(nameof(CheckSessions), Updater);
        }

        private void UpdateLicense()
        {
            var license = new LicenseGenerator(Secret, HDDidChecked ? HDDid : null, SelectedDateChecked ? ExpirationDate : null, IssuedFor, CheckSessions);
            LicenseText = license.GetLicenseEncryptedRow();
        }

        private void CreateLicense()
        {
            var write_dialog = new SaveFileDialog
            {
                Title = "Save license file",
                DefaultExt = ".lic",
                Filter = "license (*.lic)|*.lic|text files (*.txt)|*.txt|All files (*.*)|*.*",
                AddExtension = true,
                OverwritePrompt = true,
                RestoreDirectory = true,
                ValidateNames = true,
                FileName = "license.lic"
            };

            if (write_dialog.ShowDialog() != true)
                return;
            var license = new LicenseGenerator(Secret, HDDidChecked ? HDDid : null, SelectedDateChecked ? ExpirationDate : null, IssuedFor, CheckSessions);
            license.CreateLicenseFile(write_dialog.FileName);
        }
    }
}
