using System;
using System.Collections.Generic;
using System.IO;
using SessionLicenseControl;
using SessionLicenseControl.Exceptions;
using SessionLicenseControl.Licenses;
using SessionLicenseControl.Sessions;

namespace ConsoleTestSession
{
    class Program
    {
        private static string SessionsFilePath => "license.zip";
        private static string Secret => "Secret";
        static void Main(string[] args)
        {
            Test();
        }

        static void Test()
        {
            //TODO UNCOMMENT THIS TO CREATE TEST LICENSE FILE
            //if (!File.Exists(SessionsFilePath))
            //{
            //    var lic = new LicenseGenerator(Secret, License.GetThisPcHddSerialNumber(), DateTime.Now.AddDays(10), License.DefaultIssuedFor, true);
            //    lic.CreateLicenseFile(SessionsFilePath);
            //}
            var flag = true;
            string row = null;
            while (flag)
                try
                {
                    var controller = row is null ? new SessionLicenseController(SessionsFilePath, Secret, true, "Admin") : new SessionLicenseController(row, Secret, SessionsFilePath, true, "Admin");
                    flag = false;
                    "License information:".ConsoleYellow();
                    controller.License.ToString().ConsoleRed();

                    if (!controller.License.IsValid)
                    {
                        throw new InvalidLicenseException("License NOT VALID", nameof(License.ValidateLicense));
                    }

                    foreach (var (date_time, sessions) in controller.SessionController.GetSessionData())
                    {
                        $"Day: {date_time:dd.MM.yyyy}".ConsoleYellow();
                        foreach (var session in sessions)
                        {
                            session.ConsoleRed();
                        }
                    }

                    Console.WriteLine(controller.IsValid ? "License is normal" : "License is bad");


                    controller.CloseSession();
                }
                catch (Exception)
                {
                    Console.WriteLine("License is bad, Enter license code or add file");
                    row = Console.ReadLine();
                }

        }
        private static void CheckTestFiles(string filePath)
        {
            var license = new License(new FileInfo(filePath), Secret);

            "License information:".ConsoleYellow();
            license.ToString().ConsoleRed();

            Console.WriteLine(license.IsValid ? "License is normal" : "License is bad");
        }

        static SessionsOperator OpenSessions(bool NeedCover)
        {
            var result = new SessionsOperator(SessionsFilePath, true, "Admin", NeedCover ? Secret : null, true);
            "Start session test".ConsoleGreen();
            Console.WriteLine();

            foreach (var session in result.Sessions)
            {
                $"Day: {session.Date:dd.MM.yyyy}".ConsoleYellow();
                foreach (var (start_time, end_time, user, info) in session.Sessions)
                {
                    if (start_time == result.CurrentDay.LastSession.StartTime)
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

            return result;
        }
    }
}
