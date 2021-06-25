using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace SessionLicenseControl.Session
{
    public class DaySessions
    {
        /// <summary> Count of seconds per  day </summary>
        [JsonIgnore]
        public const int DayMaxSeconds = 86400;
        [JsonIgnore]
        public WorkSession CurrentSession;

        public DaySessions()
        {

        }

        /// <summary>
        /// Create new Watcher
        /// </summary>
        /// <param name="date">current date</param>
        /// <param name="UserName">User</param>
        public DaySessions(DateTime date, string UserName)
        {
            Date = date.Date;
            StartNewSession(date,UserName);
        }

        /// <summary> Sessions date </summary>
        public DateTime Date { get; set; }

        /// <summary> Sessions data for this day </summary>
        public List<WorkSession> Sessions { get; } = new();

        /// <summary> Sessions count for this day </summary>
        public int GetSessionsCount() => Sessions.Count;

        public void CloseCurrentSession()
        {
            CurrentSession.CloseSession();
            CurrentSession = null;
        }

        /// <summary>
        /// Add new session for this day
        /// </summary>
        /// <param name="date">new session start time</param>
        /// <param name="UserName"></param>
        public WorkSession StartNewSession(DateTime date, string UserName)
        {
            var last_session = Sessions.LastOrDefault();
            if (last_session != null && last_session.EndTime is null)
                last_session.EndTime = date - TimeSpan.FromSeconds(1);

            CurrentSession = new WorkSession(date, UserName);
            Sessions.Add(CurrentSession);
            return CurrentSession;
        }
    }

}
