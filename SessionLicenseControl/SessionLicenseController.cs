using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using SessionLicenseControl.Exceptions;
using SessionLicenseControl.Session;

namespace SessionLicenseControl
{
    public class SessionLicenseController
    {
        private readonly string _FilePath;
        private readonly string _Secret;
        public bool IsValid => License is not null && License.IsValid;
        public WorkSession CurrentSession { get; private set; }
        public SessionLicenseModel License { get; private set; }

        public SessionLicenseController()
        {

        }

        public SessionLicenseController(string FilePath, string Secret, bool NeedStartNewSession, string UserName)
        {
            _FilePath = FilePath;
            _Secret = Secret;

            LoadData(FilePath, Secret, UserName, NeedStartNewSession);
            if (!NeedStartNewSession) return;
            StartNewSession(UserName);
            SaveData(_FilePath, _Secret);
        }

        #region Save/Load

        /// <summary> Load data from the file </summary>
        public async Task LoadDataAsync(string FilePath, string Secret, string UserName, bool NeedStartNewSession)
        {
            try
            {
                var file_path = FilePath;
                if (!File.Exists(file_path))
                    throw new FileNotFoundException(FilePath, "License file not found");

                if (Secret is null)
                    throw new ArgumentNullException(nameof(Secret), "Secret row can't be null");
                var file = new FileInfo(file_path);

                var time_out_count = 0;
                while (file.IsLocked() && time_out_count < 100)
                {
                    await Task.Delay(300);
                    time_out_count++;
                }

                var session_text = await File.ReadAllTextAsync(file_path, Encoding.UTF8);
                License = session_text.Decrypt<SessionLicenseModel>(true, Secret);
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
        public void LoadData(string FilePath, string Secret, string UserName, bool NeedStartNewSession) => LoadDataAsync(FilePath, Secret, UserName,  NeedStartNewSession).Wait();

        /// <summary> Save data to the file </summary>
        public bool SaveData(string FilePath, string Secret) => SaveDataAsync(FilePath,Secret).Result;
        /// <summary> Save data to the file </summary>
        public async Task<bool> SaveDataAsync(string FilePath, string Secret)
        {
            var data = this.Encrypt(true, Secret);

            var file = new FileInfo(FilePath);
            file.CreateParentIfNotExist();
            var time_out_count = 0;
            while (file.IsLocked() && time_out_count < 100)
            {
                await Task.Delay(300);
                time_out_count++;
            }

            await File.WriteAllTextAsync(FilePath, data);
            return true;
        }

        #endregion

        public void StartNewSession(string UserName)
        {
            if (License.Sessions.Count > 0 && License.Sessions[^1] is { } day && day.Date.Date == DateTime.Now.Date)
            {
                CurrentSession = day.StartNewSession(DateTime.Now, UserName);
            }
            else
            {
                var session = new DaySessions(DateTime.Now, UserName);
                CurrentSession = session.Sessions.Last();
                License.Sessions.Add(session);
            }

        }
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
            CloseLastSession();
            await SaveDataAsync(_FilePath,_Secret);
        }
        /// <summary>
        /// Close session
        /// </summary>
        private void CloseLastSession()
        {
            if (CurrentSession is null)
                throw new SessionExceptions("No sessions", nameof(CloseSession));
            CurrentSession.EndTime ??= DateTime.Now;

        }

    }
}