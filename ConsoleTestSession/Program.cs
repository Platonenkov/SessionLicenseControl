using System;
using SessionLicenseControl.Session;

namespace ConsoleTestSession
{
    class Program
    {
        private static string SessionsFilePath => "Sessions.slc";
        private static bool NeedCover => true;
        private static string CoverRow => "ConsoleTest";
        static void Main(string[] args)
        {
            var sessions = new SessionsOperator(SessionsFilePath, true,"Admin", NeedCover, CoverRow);
            "Start session test".ConsoleGreen();
            Console.WriteLine();

            foreach (var session in sessions.Sessions)
            {
                $"Day: {session.Date:dd.MM.yyyy}".ConsoleYellow();
                foreach (var (start_time, end_time, user) in session.Sessions)
                {
                    if (start_time == session.CurrentSession.StartTime)
                        $"start at {start_time}, {user}".ConsoleYellow();
                    else
                        $"start at {start_time}, end at {end_time} - User: {user} - Current session".ConsoleRed();
                }

            }
            
            Console.ReadKey();
            sessions.CloseSession();

        }
    }
}
