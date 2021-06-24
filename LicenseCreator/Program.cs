using System;
using System.Globalization;

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
                var license = new LicenseController(LicenseFileName);

                #region SET HDD

                if (CheckResponce("\nEnter the HDD signature number if you want to bind the license to the disk"
                                      + "\nIf it for this PC - Enter 'T'"
                                      + "\nPress 'Enter' if you want skip this", "", out var hdd, ConsoleColor.Yellow))
                {
                    if (hdd.ToUpper() == "T" || hdd.ToUpper() == "Т")
                    {
                        license.GetForThisPC();
                        $"Your HDD Id: {license.HDDid}".ConsoleGreen();
                    }
                    else license.HDDid = hdd;
                }
                if (license.EnableHDD)
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
                        "", out var date, ConsoleColor.Yellow)||date=="")
                    {
                        if(date == "")
                            break;
                        if (int.TryParse(date, out var days))
                        {
                            license.ExpirationDate = DateTime.Now.Date.AddDays(days);
                            break;
                        }

                        if (ToDate(date, out var date_time, new[] { "dd.MM.yyyy", "dd.MM.yyyy HH:mm", "dd.MM.yy", "dd.MM.yy HH:mm" }))
                        {
                            license.ExpirationDate = date_time;
                            break;
                        }

                        "Wrong date format".ConsoleRed();
                    }

                }

                if (license.EnableDate)
                    Console.WriteLine();
                else
                    "DATA CHECK - DISABLED".ConsoleRed();

                #endregion

                #region SET SECRETS

                if (CheckResponce(
                    "Input secret word to cover license code, or press 'Enter' to set empty row",
                    null, out var secret, ConsoleColor.Yellow, true))
                    license.Secret = secret;
                Console.WriteLine();

                #endregion

                Console.WriteLine(license.GetLicenseCodeRow());
                if (license.CreateLicenseFile())
                {
                    $"File successfuly created: {license.FileName}".ConsoleYellow();
                }
                switch (license.EnableDate)
                {
                    case true when license.EnableHDD:
                        $"License created for HDD: {license.HDDid}, expires {license.ExpirationDate}".ConsoleRed();
                        break;
                    case false when !license.EnableHDD:
                        "UNLIMITED license was created".ConsoleRed();
                        break;
                    case false:
                        $"UNLIMITED license was created for PC with HDD: {license.HDDid}".ConsoleRed();
                        break;
                    default:
                        $"license has been created, expires {license.ExpirationDate:dd.MM.yyyy HH:mm} for any PC".ConsoleRed();
                        break;
                }

            }

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
