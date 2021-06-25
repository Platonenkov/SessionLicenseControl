using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MathCore.Annotations;
using SessionLicenseControl.Exceptions;

namespace SessionLicenseControl.Session
{
    public class SessionsOperator
    {
        private readonly string _FilePath;
        public WorkSession LastSession { get; private set; }
        public SessionsOperator(string FilePath, bool NeedStartNewSession, string UserName, string CoverRow)
        {
            _FilePath = FilePath;
            LoadData(CoverRow);

            if (!NeedStartNewSession) return;
            StartNewSession(UserName);
            SaveData(CoverRow);
        }

        public void StartNewSession(string UserName)
        {
            if (Sessions.Count > 0 && Sessions[^1] is { } day && day.Date.Date == DateTime.Now.Date)
            {
                LastSession = day.StartNewSession(DateTime.Now, UserName);
            }
            else
            {
                var session = new DaySessions(DateTime.Now, UserName);
                LastSession = session.Sessions.Last();
                Sessions.Add(session);
            }

        }

        public List<DaySessions> Sessions = new();

        /// <summary> Save sessions data to the file </summary>
        /// <param name="CoverRow"></param>
        public bool SaveData(string CoverRow) => SaveDataAsync(CoverRow).Result;

        /// <summary> Save sessions data to the file </summary>
        /// <param name="CoverRow"></param>
        public async Task<bool> SaveDataAsync([CanBeNull] string CoverRow)
        {
            try
            {
                var data = Sessions.EncryptToRow(CoverRow is not null, CoverRow);

                var file = new FileInfo(_FilePath);
                file.CreateParentIfNotExist();
                var time_out_count = 0;
                while (file.IsLocked() && time_out_count < 100)
                {
                    await Task.Delay(300);
                    time_out_count++;
                }

                await File.WriteAllTextAsync(_FilePath, data, Encoding.UTF8);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary> Load sessions data from the file </summary>
        /// <param name="CoverRow"></param>
        public void LoadData(string CoverRow) => LoadDataAsync(CoverRow).Wait();

        /// <summary> Load sessions data from the file </summary>
        /// <param name="CoverRow"></param>
        public async Task LoadDataAsync(string CoverRow)
        {
            try
            {
                var file_path = _FilePath;
                if (!File.Exists(file_path))
                {
                    Sessions = new List<DaySessions>();
                    return;
                }

                var file = new FileInfo(file_path);
                var time_out_count = 0;
                while (file.IsLocked() && time_out_count < 100)
                {
                    await Task.Delay(300);
                    time_out_count++;
                }

                var session_text = await File.ReadAllTextAsync(file_path, Encoding.UTF8);
                var data = session_text.DecryptRow<List<DaySessions>>(CoverRow is not null, CoverRow);
                Sessions = data ?? new List<DaySessions>();
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
        public void CloseSession(string CoverRow) => CloseSessionAsync(CoverRow).Wait();
        /// <summary>
        /// Close session and save data
        /// </summary>
        /// <returns></returns>
        public async Task CloseSessionAsync(string CoverRow)
        {
            CloseLastSession();
            await SaveDataAsync(CoverRow);
        }
        /// <summary>
        /// Close session
        /// </summary>
        private void CloseLastSession()
        {
            if (LastSession is null)
                throw new SessionExceptions("No sessions", nameof(CloseSession));
            LastSession.EndTime ??= DateTime.Now;
            LastSession = null;
        }
    }
}
