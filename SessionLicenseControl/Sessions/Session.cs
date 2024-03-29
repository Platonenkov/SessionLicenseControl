﻿using System;

namespace SessionLicenseControl.Sessions
{
    /// <summary>
    /// Session class
    /// </summary>
    public class Session
    {
        public const string SessionDirectory = "Sessions";
        public const string SessionFileExtension = ".ss";
        public Session()
        {

        }

        public Session(DateTime date)=> StartTime = date;

        public Session(DateTime date, string userName, string information = null) : this(date)
        {
            Information = information;
            UserName = userName;
        }

        /// <summary> Session start time</summary>
        public DateTime StartTime { get; set; }
        /// <summary> Session end time </summary>
        public DateTime? EndTime { get; set; }
        /// <summary> User name for current session </summary>
        public string UserName { get; set; }
        /// <summary> Some information about session </summary>
        public string Information { get; set; }

        /// <summary> Get session duration </summary>
        public virtual TimeSpan GetDuration() => (EndTime ?? DateTime.Now) - StartTime;

        /// <summary> Close session </summary>
        /// <param name="date">Date time end session</param>
        public void CloseSession(DateTime date) { EndTime ??= date; }

        /// <summary> Close session </summary>
        public void CloseSession() => EndTime ??= DateTime.Now;

        public void Deconstruct(out DateTime start_time, out DateTime? end_time, out string user, out string info) =>
            (start_time, end_time, user, info) = (StartTime, EndTime, UserName, Information);
    }
}
