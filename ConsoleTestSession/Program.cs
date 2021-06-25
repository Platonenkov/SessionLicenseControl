using System;
using System.Collections.Generic;
using System.IO;
using SessionLicenseControl;
using SessionLicenseControl.Licenses;
using SessionLicenseControl.Session;

namespace ConsoleTestSession
{
    class Program
    {
        private static string SessionsFilePath => "license.lic";
        public static bool CoverSessions => true;
        private static string CoverRow => "ConsoleTest";
        private static SessionsOperator Session;
        static void Main(string[] args)
        {
            Test_1();

            Console.ReadKey();
        }

        static void Test_2()
        {
            void GenerateLicense()
            {
                var lic = new SessionLicenseController(SessionsFilePath, CoverRow, true, "admin");

            }
        }
        static void Test_1()
        {
            OpenSessions(CoverSessions);
            GenerateTestFiles();
            foreach (var file in GenerateTestFiles())
            {
                var license = new License(new FileInfo(file), CoverRow);

                "License information:".ConsoleYellow();
                license.ToString().ConsoleRed();

                Console.WriteLine(license.IsValid ? "License is normal" : "License is bad");

            }
            Session?.CloseSession(CoverSessions ? CoverRow : null);
        }

        private static IEnumerable<string> GenerateTestFiles()
        {
            var result = new List<string>();
            var license = new LicenseGenerator(new FileInfo(Path.Combine(Environment.CurrentDirectory, "TestData", "1.lic")),
                License.GetThisPcHddSerialNumber(),
                null,
                CoverRow);

            result.Add(license.CreateLicenseFile());
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

        static void OpenSessions(bool NeedCover)
        {
            Session = new SessionsOperator(SessionsFilePath, true, "Admin", NeedCover ? CoverRow : null);
            "Start session test".ConsoleGreen();
            Console.WriteLine();

            foreach (var session in Session.Sessions)
            {
                $"Day: {session.Date:dd.MM.yyyy}".ConsoleYellow();
                foreach (var (start_time, end_time, user, info) in session.Sessions)
                {
                    if (start_time == Session.LastSession.StartTime)
                    {
                        if (user.IsNotNullOrWhiteSpace())
                            $"start at {start_time}, {user} - Current session".ConsoleYellow();
                        else
                            $"start  at {start_time} - Current session".ConsoleYellow();
                    }
                    else
                    {
                        if (user.IsNotNullOrWhiteSpace())
                            $"start at {start_time}, end at {end_time} - User: {user}".ConsoleRed();
                        else
                            $"start at {start_time}, end at {end_time}".ConsoleRed();
                    }
                }
            }
        }
    }
}
