using System;
using MathCore.ViewModels;
using SessionLicenseControl.Interfaces;
using SessionLicenseControl.WPF;

namespace WpfTestLicense
{
    public class MainViewModel: ViewModel,ILicenseBase
    {
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

            LicenseChecker.OnLicenseLoaded += () =>
            {
                ExpirationDate = LicenseChecker.LicenseController.License.ExpirationDate;
                IssuedFor = LicenseChecker.LicenseController.License.IssuedFor;
                HDDid = LicenseChecker.LicenseController.License.HDDid;
                CheckSessions = LicenseChecker.LicenseController.License.CheckSessions;
                LicenseInfo = LicenseChecker.LicenseController.License.GetLicenseInformation();
            };
            LicenseChecker.CheckLicense("license.lic", "testwpf", true, "admin");
        }
    }
}
