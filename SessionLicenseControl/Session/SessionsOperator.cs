using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SessionLicenseControl.Exceptions;
using SessionLicenseControl.Information;

namespace SessionLicenseControl.Session
{
    public class SessionsOperator
    {
        public readonly string _FilePath;
        public WorkSession CurrentSession;
        public SessionsOperator(string FilePath, bool NeedStartNewSeession, string UserName, bool NeedCover, string CoverRow)
        {
            _FilePath = FilePath;
            this.NeedCover = NeedCover;
            this.CoverRow = CoverRow;
            LoadData();

            if (!NeedStartNewSeession) return;
            StartNewSession(UserName);
            SaveData();
        }

        public void StartNewSession(string UserName)
        {
            if (Sessions.Count>0 && Sessions[^1] is { } day && day.Date.Date == DateTime.Now.Date)
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

        public List<DaySessions> Sessions = new();

        public bool NeedCover { get; set; }
        public string CoverRow { get; set; }

        /// <summary> Create string data with session info </summary>
        private string CreateData() => Sessions.CreateDataRow(NeedCover, CoverRow);

        /// <summary>
        /// Get data from string
        /// </summary>
        /// <param name="data">string data</param>
        /// <returns></returns>
        private List<DaySessions> GetData(string data) => data.GetDataFromRow<List<DaySessions>>(NeedCover, CoverRow);
        /// <summary> Save sessions data to the file </summary>
        public bool SaveData()
        {
            try
            {
                var data = CreateData();

                var file = new FileInfo(_FilePath);
                var time_out_count = 0;
                while (file.IsLocked() && time_out_count < 100)
                {
                    Task.Delay(300);
                    time_out_count++;
                }

                File.WriteAllText(_FilePath, data);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary> Save sessions data to the file </summary>
        public async Task<bool> SaveDataAsync()
        {
            try
            {
                var data = CreateData();

                var file = new FileInfo(_FilePath);
                var time_out_count = 0;
                while (file.IsLocked() && time_out_count < 100)
                {
                    await Task.Delay(300);
                    time_out_count++;
                }

                await File.WriteAllTextAsync(_FilePath, data);
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }
        /// <summary> Load sessions data from the file </summary>
        public bool LoadData()
        {
            try
            {
                var file_path = _FilePath;
                if (!File.Exists(file_path))
                {
                    return false;
                }

                var file = new FileInfo(file_path);
                //TrialTimeWatcher watcher;
                var time_out_count = 0;
                while (file.IsLocked() && time_out_count < 100)
                {
                    Task.Delay(300);
                    time_out_count++;
                }

                var session_text = File.ReadAllText(file_path, Encoding.UTF8);
                var data = GetData(session_text);
                Sessions = data ?? new List<DaySessions>();
                return true;
            }
            catch (FormatException e)
            {
                throw new SessionExceptions("Invalid session string", nameof(LoadData), e);
            }
            catch (CryptographicException e)
            {
                throw new SessionExceptions("Invalid cover string", nameof(LoadData), e);
            }
        }
        /// <summary> Load sessions data from the file </summary>
        public async Task<bool> LoadDataAsync()
        {
            try
            {
                var file_path = _FilePath;
                if (!File.Exists(file_path))
                {
                    return false;
                }

                var file = new FileInfo(file_path);
                //TrialTimeWatcher watcher;
                var time_out_count = 0;
                while (file.IsLocked() && time_out_count < 100)
                {
                    await Task.Delay(300);
                    time_out_count++;
                }

                var session_text = await File.ReadAllTextAsync(file_path, Encoding.UTF8);
                var data = GetData(session_text);
                Sessions = data ?? new List<DaySessions>();
                return true;
            }
            catch (FormatException e)
            {
                throw new SessionExceptions("Invalid session string", nameof(LoadData), e);
            }
            catch (CryptographicException e)
            {
                throw new SessionExceptions("Invalid cover string", nameof(LoadData), e);
            }
        }
        /// <summary>
        /// Close session and save data
        /// </summary>
        public void CloseSession()
        {
            CloseLastSession();
            SaveData();
        }
        /// <summary>
        /// Close session and save data
        /// </summary>
        /// <returns></returns>
        public async Task CloseSessionAsync()
        {
            CloseLastSession();
            await SaveDataAsync();
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
