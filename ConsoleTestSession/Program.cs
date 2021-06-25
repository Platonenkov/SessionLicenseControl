using System;
using System.Collections.Generic;
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
            foreach (var file in GenerateTestFiles())
            {
                var license = new LicenseController(new FileInfo(file), CoverRow);

                "License information:".ConsoleYellow();
                license.ToString().ConsoleRed();

                Console.WriteLine(license.CheckLicense() ? "License is normal" : "License is bad");

            }
            Session?.CloseSession();
        }

        private static IEnumerable<string> GenerateTestFiles()
        {
            var result = new List<string>();
            var license = new LicenseController();
            license.Secret = CoverRow;
            license.SetForThisPC();
            result.Add(license.CreateLicenseFile(Path.Combine(Environment.CurrentDirectory, "TestData", "1.lic")));
            license.HDDid = "12312hsd";
            result.Add(license.CreateLicenseFile(Path.Combine(Environment.CurrentDirectory, "TestData", "2.lic")));
            license.HDDid = null;
            result.Add(license.CreateLicenseFile(Path.Combine(Environment.CurrentDirectory, "TestData", "3.lic")));
            license.ExpirationDate = DateTime.Now.AddDays(2);
            result.Add(license.CreateLicenseFile(Path.Combine(Environment.CurrentDirectory, "TestData", "4.lic")));
            license.ExpirationDate = DateTime.Now.Date;
            result.Add(license.CreateLicenseFile(Path.Combine(Environment.CurrentDirectory, "TestData", "5.lic")));
            license.ExpirationDate = DateTime.Now - TimeSpan.FromDays(1);
            result.Add(license.CreateLicenseFile(Path.Combine(Environment.CurrentDirectory, "TestData", "6.lic")));
            return result;
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
                    if (start_time == Session.CurrentSession.StartTime)
                    {
                        if (user.IsNotNullOrWhiteSpace())
                            $"start at {start_time}, {user} - Current session".ConsoleYellow();
                        else
                            $"start  at {start_time} - Current session".ConsoleYellow();
                    }                    
                    else
                    {
                        if(user.IsNotNullOrWhiteSpace())
                            $"start at {start_time}, end at {end_time} - User: {user}".ConsoleRed();
                        else
                            $"start at {start_time}, end at {end_time}".ConsoleRed();
                    }
                }
            }
        }
    }
}
