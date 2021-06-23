//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Text.Json.Serialization;
//using SessionLicenseControl.Exceptions;
//using SessionLicenseControl.Session;

//namespace SessionLicenseControl.Licenses
//{
//    public class TrialDayWatcher
//    {
//        /// <summary>
//        /// Maximum count of sessions per day
//        /// </summary>
//        [JsonIgnore]
//        public static int MaximumSessionCount = 300;

//        public TrialDayWatcher()
//        {

//        }
//        /// <summary>
//        /// Create new Watcher
//        /// </summary>
//        /// <param name="date">current date</param>
//        public TrialDayWatcher(DateTime date)
//        {
//            Day = new DaySessions(date);
//        }

//        /// <summary> Sessions day </summary>
//        public DaySessions Day { get; set; }


//        /// <summary> Receives total software operation time per day </summary>
//        /// <returns>count of seconds worked per day</returns>
//        public double GetSecondsByThisDay() => Day.Sessions.Sum(session => session.GetDuration().TotalSeconds);
//        /// <summary> Check current day sessions </summary>
//        /// <param name="date">current time or new session time</param>
//        public void CheckDaySessions(DateTime date)
//        {
//            if (GetSecondsByThisDay() > DaySessions.DayMaxSeconds)
//                throw new SessionExceptions("Maximum session duration for a day", nameof(CheckDaySessions));
//            //Maximum count of sessions per day per restart every 5 minutes
//            if (Day.GetSessionsCount() > MaximumSessionCount)
//                throw new SessionExceptions("Maximum session count per day", nameof(CheckDaySessions));
//            //checks for a starting before current session
//            if (Day.Sessions.Contains(s => s.StartTime > date))
//                throw new SessionExceptions("Contains sessions which started before current", nameof(CheckDaySessions));
//        }
//        /// <summary>
//        /// Add new session for this day
//        /// </summary>
//        /// <param name="date">new session start time</param>
//        public void StartNewSession(DateTime date)
//        {
//            CheckDaySessions(date);

//            Day.StartNewSession(date);
//        }
//    }
//}
