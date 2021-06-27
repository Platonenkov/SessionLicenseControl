using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathCore.ViewModels;
using SessionLicenseControl.Interfaces;
using SessionLicenseControl.WPF;
using SessionLicenseControl.WPF.Styles;

namespace WpfTestLicense
{
    public class MainViewModel: ViewModel,ILicenseBase
    {
        #region Status : string - Status

        /// <summary>Status</summary>
        private  string _Status;

        /// <summary>Status</summary>
        public  string Status { get => _Status; set => Set(ref _Status, value); }

        #endregion

        #region HDDid : string - hdd

        /// <summary>hdd</summary>
        private string _HDDid;

        /// <summary>hdd</summary>
        public string HDDid { get => _HDDid; set => Set(ref _HDDid, value); }

        #endregion
        #region Date : DateTime? - date

        /// <summary>date</summary>
        private DateTime? _Date;

        /// <summary>date</summary>
        public DateTime? ExpirationDate { get => _Date; set => Set(ref _Date, value); }

        #endregion

        #region CheckSessions : bool - CheckSessions

        /// <summary>CheckSessions</summary>
        private bool _CheckSessions;

        /// <summary>CheckSessions</summary>
        public bool CheckSessions { get => _CheckSessions; set => Set(ref _CheckSessions, value); }

        #endregion
        #region Owner : string - owner

        /// <summary>owner</summary>
        private string _Owner;

        /// <summary>owner</summary>
        public string IssuedFor { get => _Owner; set => Set(ref _Owner, value); }

        #endregion

        #region LicenseInfo : string - info

        /// <summary>info</summary>
        private string _LicenseInfo;

        /// <summary>info</summary>
        public string LicenseInfo { get => _LicenseInfo; set => Set(ref _LicenseInfo, value); }

        #endregion

        public MainViewModel()
        {

            ModernWindow.OnLicenseLoaded += () =>
            {
                ExpirationDate = ModernWindow.LicenseController.License.ExpirationDate;
                IssuedFor = ModernWindow.LicenseController.License.IssuedFor;
                HDDid = ModernWindow.LicenseController.License.HDDid;
                CheckSessions = ModernWindow.LicenseController.License.CheckSessions;
                LicenseInfo = ModernWindow.LicenseController.License.GetLicenseInformation();
            };
            SessionLicenseControl.WPF.StatusChecker.Checker = status => ModernWindow.LoadStyle(status,"license.lic","testwpf",true,"admin");
            Status = SessionLicenseControl.WPF.StatusChecker.CheckStatus(Initialized: false); // Не удалять и не менять положение! Инициализация механизма проверки лицензии
        }
    }
}
