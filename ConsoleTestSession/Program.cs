using System;
using System.IO;
using SessionLicenseControl.Licenses;
using SessionLicenseControl.Session;

namespace ConsoleTestSession
{
    class Program
    {
        private static string SessionsFilePath => "Sessions.slc";
        private static bool NeedCover => true;
        private static string CoverRow => "ConsoleTest";
        private static SessionsOperator Session;
        static void Main(string[] args)
        {
            Test_1();

            Console.ReadKey();
        }
        static void Test_1()
        {
            OpenSessions();
            GenerateTestFiles();
            for (var i = 1; i < 7; i++)
            {
                var license = new LicenseController(new FileInfo(Path.Combine(Environment.CurrentDirectory, "TestData", $"{i}.lic")), CoverRow);

                "License information:".ConsoleYellow();
                license.ToString().ConsoleRed();

                Console.WriteLine(license.CheckLicense() ? "License is normal" : "License is bad");

            }
            Session?.CloseSession();
        }

        private static void GenerateTestFiles()
        {
            var license = new LicenseController();
            license.Secret = CoverRow;
            license.SetForThisPC();
            license.CreateLicenseFile(Path.Combine(Environment.CurrentDirectory, "TestData", "1.lic"));
            license.HDDid = "12312hsd";
            license.CreateLicenseFile(Path.Combine(Environment.CurrentDirectory, "TestData", "2.lic"));
            license.HDDid = null;
            license.CreateLicenseFile(Path.Combine(Environment.CurrentDirectory, "TestData", "3.lic"));
            license.ExpirationDate = DateTime.Now.AddDays(2);
            license.CreateLicenseFile(Path.Combine(Environment.CurrentDirectory, "TestData", "4.lic"));
            license.ExpirationDate = DateTime.Now.Date;
            license.CreateLicenseFile(Path.Combine(Environment.CurrentDirectory, "TestData", "5.lic"));
            license.ExpirationDate = DateTime.Now - TimeSpan.FromDays(1);
            license.CreateLicenseFile(Path.Combine(Environment.CurrentDirectory, "TestData", "6.lic"));


        }

        static void OpenSessions()
        {
            Session = new SessionsOperator(SessionsFilePath, true, "Admin", NeedCover, CoverRow);
            "Start session test".ConsoleGreen();
            Console.WriteLine();

            foreach (var session in Session.Sessions)
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
        }
    }
}
