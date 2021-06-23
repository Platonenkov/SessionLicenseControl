using System;

namespace SessionLicenseControl.Session
{
    /// <summary>
    /// Session class
    /// </summary>
    public class WorkSession
    {
        public WorkSession()
        {

        }

        public WorkSession(DateTime date) => StartTime = date;
        public WorkSession(DateTime date, string userName) : this(date) => UserName = userName;

        /// <summary> Session start time</summary>
        public DateTime StartTime { get; set; }
        /// <summary> Session end time </summary>
        public DateTime? EndTime { get; set; }
        /// <summary> User name for current session </summary>
        public string UserName { get; set; }
        /// <summary> Get session duration </summary>
        public virtual TimeSpan GetDuration() => (EndTime ?? DateTime.Now) - StartTime;

        /// <summary> Close session </summary>
        /// <param name="date">Date time end session</param>
        public void CloseSession(DateTime date) { EndTime ??= date; }

        /// <summary> Close session </summary>
        public void CloseSession() => EndTime ??= DateTime.Now;

        public void Deconstruct(out DateTime start_time, out DateTime? end_time, out string user) =>
            (start_time, end_time, user) = (StartTime, EndTime, UserName);
    }
}
