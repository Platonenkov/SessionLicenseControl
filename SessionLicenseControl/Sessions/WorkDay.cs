using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using MathCore.Annotations;

namespace SessionLicenseControl.Sessions
{
    public class WorkDay
    {
        /// <summary> Count of seconds per  day </summary>
        [JsonIgnore]
        public const int DayMaxSeconds = 86400;
        [JsonIgnore]
        public Session LastSession { get; private set; }
        public WorkDay()
        {

        }

        /// <summary>
        /// Create new Watcher
        /// </summary>
        /// <param name="date">current date</param>
        /// <param name="UserName">User</param>
        public WorkDay(DateTime date, string UserName)
        {
            Date = date.Date;
            StartNewSession(date,UserName);
        }

        /// <summary> Sessions date </summary>
        public DateTime Date { get; set; }

        /// <summary> Sessions data for this day </summary>
        public List<Session> Sessions { get; set; } = new();

        /// <summary> Sessions count for this day </summary>
        public int GetSessionsCount() => Sessions.Count;

        /// <summary>
        /// Add new session for this day
        /// </summary>
        /// <param name="date">new session start time</param>
        /// <param name="UserName"></param>
        [NotNull]
        public Session StartNewSession(DateTime date, string UserName)
        {
            var last_session = Sessions.LastOrDefault();
            if (last_session is {EndTime: null})
            {
                last_session.EndTime = date - TimeSpan.FromSeconds(1);
                last_session.Information ??= "Was close when start new";
            }
            var new_session = new Session(date, UserName);
            LastSession = new_session;
            Sessions.Add(LastSession);
            return LastSession;
        }
    }

}
