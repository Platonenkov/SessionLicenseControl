using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using SessionLicenseControl.Exceptions;
using SessionLicenseControl.Extensions;

namespace SessionLicenseControl.Sessions
{
    public class SessionsOperator
    {
        private readonly string _SessionArchiveFilePath;

        public WorkDay CurrentDay { get; private set; }

        /// <summary> List of days with sessions </summary>
        public List<WorkDay> Sessions;
        /// <summary> secret row to cover code </summary>
        private string Secret { get; }

        public static string GetCurrentDayFileName() => $"{Session.SessionDirectory}\\{DateTime.Now.Date:yy-MM-dd}{Session.SessionFileExtension}";
        /// <summary>
        /// Initialize new session controller
        /// </summary>
        /// <param name="SessionArchiveFilePath">path to archive file</param>
        /// <param name="NeedStartNewSession">start new session when initialize</param>
        /// <param name="UserName">user name who login now</param>
        /// <param name="secret">secret row to cover code</param>
        /// <param name="ThrowIfNull">throw error if session was deleted or cleared</param>
        public SessionsOperator(string SessionArchiveFilePath, bool NeedStartNewSession, string UserName, string secret,bool ThrowIfNull)
        {
            _SessionArchiveFilePath = SessionArchiveFilePath;
            Secret = secret;
            LoadData();
            SetCurrentDay();
            if (ThrowIfNull && Sessions is not {Count: >0})
                throw new SessionExceptions("Sessions was cleared", nameof(Session));

            if (!NeedStartNewSession) return;
            StartNewSession(UserName);
            SaveData();
        }
        /// <summary>
        /// Проверка лицензии
        /// </summary>
        /// <exception cref="SessionLicenseExceptions">if day contains session for other day</exception>
        public bool ValidateSessions(DateTime? ExpirationDate)
        {
            if (Sessions is null || Sessions.Count == 0)
                return true;
            var start_day = Sessions.First();
            if (start_day.Date > DateTime.Now)
                return false;
            if (ExpirationDate is null)
                return true;

            var trial_days = (ExpirationDate - start_day.Date).Value.Days;
            if (trial_days - Sessions.Count < 0) //if the user tried to change the date
                return false;

            var first_session = start_day.Sessions.First().StartTime.Date;
            if (first_session.Date != start_day.Date)
                throw new SessionLicenseExceptions($"Invalid data in session: {first_session} in date {start_day.Date:dd.MM.yyyy}", nameof(ValidateSessions));

            var total_days = (DateTime.Now - first_session).Days; //total timer
            if (total_days > trial_days)
                return false;

            return true;
        }


        private void SetCurrentDay()
        {
            if (Sessions.Count > 0 && Sessions[^1] is { } day && day.Date.Date == DateTime.Now.Date)
            {
                CurrentDay = day;
            }
        }
        public void StartNewSession(string UserName)
        {

            if (CurrentDay is not null)
            {
                CurrentDay.StartNewSession(DateTime.Now, UserName);
            }
            else
            {
                CurrentDay = new WorkDay(DateTime.Now, UserName);
                Sessions.Add(CurrentDay);
            }

        }

        /// <summary> Save sessions data to the file </summary>
        private void SaveData() => CurrentDay.SaveToZipFile(_SessionArchiveFilePath, GetCurrentDayFileName(), Secret);

        /// <summary> Save sessions data to the file </summary>
        private async Task SaveDataAsync() => await CurrentDay.SaveToZipFileAsync(_SessionArchiveFilePath, GetCurrentDayFileName(), Secret);

        /// <summary> Load sessions data from the file </summary>
        private void LoadData()
        {
            try
            {
                var file = new FileInfo(_SessionArchiveFilePath);
                if (!file.Exists)
                {
                    Sessions = new List<WorkDay>();
                    return;
                }

                Sessions = file.GetAllFromZip<WorkDay>(Session.SessionFileExtension, Secret);
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
        private async Task LoadDataAsync()
        {
            try
            {
                var file = new FileInfo(_SessionArchiveFilePath);
                if (!file.Exists)
                {
                    Sessions = new List<WorkDay>();
                    return;
                }

                Sessions = await file.GetAllFromZipAsync<WorkDay>(Session.SessionFileExtension, Secret);
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
        public void CloseLastSession()
        {
            if (CurrentDay?.LastSession is null)
                throw new SessionExceptions("No sessions", nameof(CloseSession));
            CurrentDay.LastSession.EndTime ??= DateTime.Now;
        }

        public Dictionary<DateTime, IEnumerable<string>> GetSessionData()
        {
            var sessions = new Dictionary<DateTime, IEnumerable<string>>();
            foreach (var session in Sessions)
            {
                var day_sessions = new List<string>();
                foreach (var (start_time, end_time, user, info) in session.Sessions)
                {
                    if (start_time == CurrentDay.LastSession.StartTime)
                    {
                        if (user.IsNotNullOrWhiteSpace())
                        {
                            day_sessions.Add(
                                info.IsNotNullOrWhiteSpace()
                                    ? $"start at {start_time}, User: {user} - Current session, Information: {info}"
                                    : $"start at {start_time}, User: {user} - Current session");
                        }
                        else
                        {
                            day_sessions.Add(
                                info.IsNotNullOrWhiteSpace()
                                    ? $"start at {start_time} - Current session, Information: {info}"
                                    : $"start  at {start_time} - Current session");
                        }
                    }
                    else
                    {
                        if (user.IsNotNullOrWhiteSpace())
                        {
                            day_sessions.Add(
                                info.IsNotNullOrWhiteSpace()
                                    ? $"start at {start_time}, end at {end_time}, User: {user}, Information: {info}"
                                    : $"start at {start_time}, end at {end_time}, User: {user}");
                        }
                        else
                        {
                            day_sessions.Add(
                                info.IsNotNullOrWhiteSpace()
                                    ? $"start at {start_time}, end at {end_time}, Information: {info}"
                                    : $"start at {start_time}, end at {end_time}");
                        }
                    }
                }
                sessions.Add(session.Date,day_sessions);
            }

            return sessions;

        }

    }
}
