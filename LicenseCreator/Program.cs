using System;
using System.Globalization;
using System.IO;
using SessionLicenseControl.Licenses;
using SessionLicenseControl.Licenses;

namespace LicenseCreator
{

    class Program
    {
        private static string LicenseFileName = "License.lic";
        static void Main(string[] args)
        {
            var itFirst = true;
            while (itFirst || GetKeyFromConsole("\nPress 'Y' to create new license, press any key to close.", ConsoleColor.Green) == ConsoleKey.Y)
            {
                itFirst = false;
                var key = GetKeyFromConsole("\nPress 1 to create new license"
                                            + "\nPress 2 to create license with session validation"
                                            + "\nPress any key to close.", ConsoleColor.Green);

                var flag = false;
                switch (key)
                {
                    case ConsoleKey.D1:
                        OnlyLicense();
                        break;
                    case ConsoleKey.D2:
                        LicenseAndSession();
                        break;
                    default: flag = true;
                        break;
                }
                if(flag)
                    break;
            }
        }

        static void OnlyLicense()
        {
            string HDD_result = null;
            DateTime? Date_result = null;
            #region SET HDD

            if (CheckResponce("\nEnter the HDD signature number if you want to bind the license to the disk"
                                  + "\nIf it for this PC - Enter 'T'"
                                  + "\nPress 'Enter' if you want skip this", "", out var hdd, ConsoleColor.Yellow))
            {
                if (hdd.ToUpper() == "T" || hdd.ToUpper() == "Т")
                {
                    HDD_result = License.GetThisPcHddSerialNumber();
                    $"Your HDD Id: {HDD_result}".ConsoleGreen();
                }
                else HDD_result = hdd;
            }
            if (HDD_result is not null)
                Console.WriteLine();
            else
                "HDD CHECK - DISABLED".ConsoleRed();
            #endregion

            #region SET DATE

            while (true)
            {
                if (CheckResponce(
                    "Input Day count for license, or input date in formats:"
                    + "\n'dd.MM.yyyy' | 'dd.MM.yy' | 'dd.MM.yyyy HH:mm' | 'dd.MM.yy HH:mm' if you want to bind the license to the date."
                    + "\nPress 'Enter' if you want skip this",
                    "", out var date, ConsoleColor.Yellow) || date == "")
                {
                    if (date == "")
                        break;
                    if (int.TryParse(date, out var days))
                    {
                        Date_result = DateTime.Now.Date.AddDays(days);
                        break;
                    }

                    if (ToDate(date, out var date_time, new[] { "dd.MM.yyyy", "dd.MM.yyyy HH:mm", "dd.MM.yy", "dd.MM.yy HH:mm" }))
                    {
                        Date_result = date_time;
                        break;
                    }

                    "Wrong date format".ConsoleRed();
                }

            }

            if (Date_result is not null)
                Console.WriteLine();
            else
                "DATA CHECK - DISABLED".ConsoleRed();

            #endregion

            #region SET SECRETS

            CheckResponce(
                "Input secret word to cover license code, or press 'Enter' to set empty row",
                null, out var secret, ConsoleColor.Yellow, true);
            Console.WriteLine();

            #endregion

            var license = new LicenseGenerator(new FileInfo(LicenseFileName), HDD_result, Date_result, secret);
            Console.WriteLine(license.GetLicenseCodeRow());
            if (license.CreateLicenseFile(LicenseFileName) is {Length:>0} file)
            {
                $"File successful created: {file}".ConsoleYellow();
            }

            #region Result message

            license.ToString().ConsoleRed();

            #endregion

        }

        static void LicenseAndSession()
        {

        }
        /// Extension method parsing a date string to a DateTime? <para/>
        /// <summary>
        /// </summary>
        /// <param name="dateTimeStr">The date string to parse</param>
        /// <param name="date">Parsed DateTime</param>
        /// <param name="dateFmt">dateFmt is optional and allows to pass 
        /// a parsing pattern array or one or more patterns passed 
        /// as string parameters</param>
        /// <returns>parse result</returns>
        public static bool ToDate(string dateTimeStr, out DateTime date, params string[] dateFmt)
        {
            // example: var dt = "2011-03-21 13:26".ToDate(new string[]{"yyyy-MM-dd HH:mm", 
            //                                                  "M/d/yyyy h:mm:ss tt"});
            // or simpler: 
            // var dt = "2011-03-21 13:26".ToDate("yyyy-MM-dd HH:mm", "M/d/yyyy h:mm:ss tt");
            const DateTimeStyles style = DateTimeStyles.AllowWhiteSpaces;
            if (dateFmt == null)
            {
                var dateInfo = System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat;
                dateFmt = dateInfo.GetAllDateTimePatterns();
            }
            var result = DateTime.TryParseExact(dateTimeStr, dateFmt, CultureInfo.InvariantCulture,
                style, out date);

            return result;
        }
        static string EnterSecret()
        {
            var secret = string.Empty;
            ConsoleKey key;
            do
            {
                var keyInfo = Console.ReadKey(intercept: true);
                key = keyInfo.Key;

                if (key == ConsoleKey.Backspace && secret.Length > 0)
                {
                    Console.Write("\b \b");
                    secret = secret[0..^1];
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    Console.Write("*");
                    secret += keyInfo.KeyChar;
                }
            } while (key != ConsoleKey.Enter);

            return secret;
        }
        static bool CheckResponce(string info, string validation, out string result, ConsoleColor color = default, bool InputAsSecret = false)
        {
            result = GetFromConsole(info, color, InputAsSecret);
            return validation is null || result.ToUpper() != validation.ToUpper();
        }
        static string GetFromConsole(string info, ConsoleColor color = default, bool InputAsSecret = false)
        {
            if (color != default)
                Console.ForegroundColor = color;
            Console.WriteLine(info);
            Console.ResetColor();

            return InputAsSecret ? EnterSecret() : Console.ReadLine();
        }
        static ConsoleKey GetKeyFromConsole(string info, ConsoleColor color = default)
        {
            if (color != default)
                Console.ForegroundColor = color;
            Console.WriteLine(info);
            return Console.ReadKey().Key;
        }

    }
}
