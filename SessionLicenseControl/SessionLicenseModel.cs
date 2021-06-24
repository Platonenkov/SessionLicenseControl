using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using SessionLicenseControl.Exceptions;
using SessionLicenseControl.Licenses;
using SessionLicenseControl.Session;

namespace SessionLicenseControl
{
    public class SessionLicenseModel
    {
        public List<DaySessions> Sessions { get; set; }
        public License License { get; set; }

        public SessionLicenseModel()
        {

        }

        public SessionLicenseModel(string FilePath, string Secret) => LoadData(FilePath, Secret);

        #region Save/Load

        /// <summary> Load data from the file </summary>
        public async Task LoadDataAsync(string FilePath, string Secret)
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
                var data = session_text.GetDataFromRow<SessionLicenseModel>(true, Secret);
                Sessions = data?.Sessions;
                License = data?.License;
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
        public void LoadData(string FilePath, string Secret) => LoadDataAsync(FilePath, Secret).Wait();

        /// <summary> Save data to the file </summary>
        public bool SaveData(string FilePath, string Secret) => SaveDataAsync(FilePath,Secret).Result;
        /// <summary> Save data to the file </summary>
        public async Task<bool> SaveDataAsync(string FilePath, string Secret)
        {
            var data = this.CreateDataRow(true, Secret);

            var file = new FileInfo(FilePath);
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
        private WorkSession CurrentSession;

        public void StartNewSession(string UserName)
        {
            if (Sessions.Count > 0 && Sessions[^1] is { } day && day.Date.Date == DateTime.Now.Date)
            {
                CurrentSession = day.StartNewSession(DateTime.Now, UserName);
            }
            else
            {
                var session = new DaySessions(DateTime.Now, UserName);
                CurrentSession = session.CurrentSession;
                Sessions.Add(session);
            }

        }

    }
}
