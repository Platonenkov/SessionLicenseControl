﻿using System;
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
        private static string SessionsFilePath => "license.dll";
        private static string CoverRow => "ConsoleTest";
        static void Main(string[] args)
        {
            //var session = OpenSessions(CoverSessions);
            //Test_1();
            Test_2();
            //Console.ReadKey();
            //session.CloseSession();
        }

        static void Test_2()
        {
            if (!File.Exists(SessionsFilePath))
            {
                var lic = new LicenseGenerator(CoverRow, License.GetThisPcHddSerialNumber(), DateTime.Now.AddDays(10), true);
                lic.CreateLicenseFile(SessionsFilePath);
            }

            var controller = new SessionLicenseController(SessionsFilePath, CoverRow, true, "Admin");
            "License information:".ConsoleYellow();
            controller.License.ToString().ConsoleRed();

            if (!controller.License.IsValid)
            {
                throw new InvalidLicenseException("License NOT VALID", nameof(License.ValidateLicense));
            }

            foreach (var session in controller.SessionController.Sessions)
            {
                $"Day: {session.Date:dd.MM.yyyy}".ConsoleYellow();
                foreach (var (start_time, end_time, user, info) in session.Sessions)
                {
                    if (start_time == controller.SessionController.CurrentDay.LastSession.StartTime)
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

            Console.WriteLine(controller.IsValid ? "License is normal" : "License is bad");


            controller.CloseSession();
        }
        //static void Test_1()
        //{
        //    foreach (var file in GenerateTestFiles(false))
        //        CheckTestFiles(file);

        //}
        private static void CheckTestFiles(string filePath)
        {
            var license = new License(new FileInfo(filePath), CoverRow);

            "License information:".ConsoleYellow();
            license.ToString().ConsoleRed();

            Console.WriteLine(license.IsValid ? "License is normal" : "License is bad");
        }
        //private static IEnumerable<string> GenerateTestFiles(bool WithSessionControl)
        //{
        //    var result = new List<string>();
        //    var license = new LicenseGenerator(new FileInfo(Path.Combine(Environment.CurrentDirectory, "TestData", "1.lic")),
        //        License.GetThisPcHddSerialNumber(),
        //        null,
        //        CoverRow);

        //    result.Add(license.CreateLicenseFile(WithSessionControl));
        //    license.HDDid = "12312hsd";
        //    result.Add(license.CreateLicenseFile(Path.Combine(Environment.CurrentDirectory, "TestData", "2.lic"), WithSessionControl));
        //    license.HDDid = null;
        //    result.Add(license.CreateLicenseFile(Path.Combine(Environment.CurrentDirectory, "TestData", "3.lic"), WithSessionControl));
        //    license.ExpirationDate = DateTime.Now.AddDays(2);
        //    result.Add(license.CreateLicenseFile(Path.Combine(Environment.CurrentDirectory, "TestData", "4.lic"), WithSessionControl));
        //    license.ExpirationDate = DateTime.Now.Date;
        //    result.Add(license.CreateLicenseFile(Path.Combine(Environment.CurrentDirectory, "TestData", "5.lic"), WithSessionControl));
        //    license.ExpirationDate = DateTime.Now - TimeSpan.FromDays(1);
        //    result.Add(license.CreateLicenseFile(Path.Combine(Environment.CurrentDirectory, "TestData", "6.lic"), WithSessionControl));
        //    return result;
        //}

        static SessionsOperator OpenSessions(bool NeedCover)
        {
            var result = new SessionsOperator(SessionsFilePath, true, "Admin", NeedCover ? CoverRow : null);
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
