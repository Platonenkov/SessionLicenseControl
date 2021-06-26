using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using SessionLicenseControl.Exceptions;
using SessionLicenseControl.Licenses;
using SessionLicenseControl.Sessions;

namespace SessionLicenseControl
{
    public class SessionLicenseController
    {
        private readonly string _FilePath;
        private readonly string _Secret;
        public bool IsValid => ValidateLicense();
        public License License { get; set; }
        public SessionsOperator SessionController { get; private set; }


        public SessionLicenseController()
        {

        }

        public SessionLicenseController(string FilePath, string Secret, bool NeedStartNewSession, string UserName)
        {
            _FilePath = FilePath;
            _Secret = Secret;

            LoadData(FilePath, Secret, NeedStartNewSession, UserName);
            if (!NeedStartNewSession) return;
            SaveData(_FilePath, _Secret);
        }

        #region Validations

        public bool ValidateLicense()
        {
            if (License?.IsValid != true)
                return false;
            if (!License.CheckSessions)
                return true;

            return SessionController.ValidateSessions(License.ExpirationDate);
        }

        #endregion
        #region Save/Load

        /// <summary> Load data from the file </summary>
        public async Task LoadDataAsync(string FilePath, string Secret, bool NeedStartNewSession, string UserName)
        {
            try
            {
                if (!File.Exists(FilePath))
                    throw new FileNotFoundException(FilePath, "License file not found");

                if (Secret is null)
                    throw new ArgumentNullException(nameof(Secret), "Secret row can't be null");
                var file = new FileInfo(FilePath);

                var time_out_count = 0;
                while (file.IsLocked() && time_out_count < 100)
                {
                    await Task.Delay(300);
                    time_out_count++;
                }

                var license = new License(file, Secret);
                await license.LoadFromFileAsync(file, Secret);
                SessionController = new SessionsOperator(FilePath, NeedStartNewSession, UserName, Secret);
                License = license;
            }
            catch (AggregateException e)
            {
                throw new SessionLicenseExceptions("string has been tampered with", nameof(LoadData), e);
            }
            catch (FormatException e)
            {
                throw new SessionLicenseExceptions("Invalid format string", nameof(LoadData), e);
            }
            catch (CryptographicException e)
            {
                throw new SessionLicenseExceptions("Invalid cover string", nameof(LoadData), e);
            }
        }

        /// <summary> Load data from the file </summary>
        public void LoadData(string FilePath, string Secret, bool NeedStartNewSession, string UserName) => LoadDataAsync(FilePath, Secret, NeedStartNewSession, UserName).Wait();

        /// <summary> Save data to the file </summary>
        public bool SaveData(string FilePath, string Secret) => SaveDataAsync(FilePath, Secret).Result;
        /// <summary> Save data to the file </summary>
        public async Task<bool> SaveDataAsync(string FilePath, string Secret)
        {
            await License.SaveToFileAsync(FilePath, Secret);
            await SessionController.CloseSessionAsync();
            return true;
        }

        #endregion

        public void StartNewSession(string UserName) => SessionController.StartNewSession(UserName);

        /// <summary>
        /// Close session and save data
        /// </summary>
        public void CloseSession() => CloseSessionAsync().Wait();
        /// <summary>
        /// Close session and save data
        /// </summary>
        /// <returns></returns>
        public async Task CloseSessionAsync()
        {
            await SaveDataAsync(_FilePath, _Secret);
        }
    }
}