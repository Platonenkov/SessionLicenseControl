using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MathCore.Annotations;
using SessionLicenseControl.Exceptions;
using SessionLicenseControl.Licenses;
using SessionLicenseControl.Session;

namespace SessionLicenseControl
{

    public class SessionLicense
    {
        /// <summary>
        /// Sessions data in license
        /// </summary>
        public List<DaySessions> Sessions { get; set; }

        /// <summary>
        /// License data
        /// </summary>
        public License License { get; set; }
        public bool IsValid => ValidateLicenseForThisPC() && ValidateSessions();

        public SessionLicense()
        {

        }
        public SessionLicense(string row, string secret)
        {
            var input = Decrypt(row, secret);
            Sessions = input.Sessions;
            License = input.License;
        }

        public SessionLicense(FileInfo file, string secret) => this.LoadFromFile(file, secret);

        #region Cryptography

        internal string Encrypt(string Secret) => this.Encrypt(true, Secret);
        public static SessionLicense Decrypt(string row, string Secret) => row.Decrypt<SessionLicense>(true, Secret);

        #endregion

        #region Validation and Checks

        #region Validate license

        /// <summary>
        /// Validate license for this pc by hdd 'C'
        /// </summary>
        /// <returns>validation result</returns>
        private bool ValidateLicenseForThisPC() => License.ValidateLicense(this.License, License.GetThisPcHddSerialNumber());

        #endregion

        #region Validate sessions

        /// <summary>
        /// Проверка лицензии
        /// </summary>
        /// <exception cref="SessionLicenseExceptions">if day contains session for other day</exception>
        private bool ValidateSessions()
        {
            if (Sessions is null || Sessions.Count == 0)
                return true;
            var start_day = Sessions.First();
            if (start_day.Date > DateTime.Now)
                return false;
            if (License?.Date is null)
                return true;

            var trial_days = (License.Date - start_day.Date).Value.Days;
            if (trial_days - Sessions.Count < 0) //if the user tried to change the date
                return false;

            var first_session = start_day.Sessions.First().StartTime.Date;
            if (first_session.Date != start_day.Date)
                throw new SessionLicenseExceptions($"Invalid data in session: {first_session} in date {start_day.Date:dd.MM.yyyy}", nameof(ValidateSessions));
            
            var total_days = (DateTime.Now - first_session).Days; //total timer
            if (total_days > trial_days)
                return false;

            return true;
        }

        #endregion

        #endregion

    }

    public static class SessionLicenseExtensions
    {
        /// <summary>
        /// Save data to the file
        /// </summary>
        /// <param name="lic">license file</param>
        /// <param name="FilePath">path where file will be save</param>
        /// <param name="secret">secret row to cover license</param>
        /// <returns>path where file was saved</returns>
        public static string SaveToFile([NotNull] this SessionLicense lic, string FilePath, string secret) => lic.SaveToFileAsync(FilePath, secret).Result;

        /// <summary>
        /// Save data to the file
        /// </summary>
        /// <param name="lic">license file</param>
        /// <param name="FilePath">path where file will be save</param>
        /// <param name="secret">secret row to cover license</param>
        /// <returns>path where file was saved</returns>
        public static async Task<string> SaveToFileAsync([NotNull] this SessionLicense lic, [NotNull] string FilePath, string secret)
        {
            var data = lic.Encrypt(secret);

            var file = new FileInfo(FilePath);
            file.CreateParentIfNotExist();
            var time_out_count = 0;
            while (file.IsLocked() && time_out_count < 100)
            {
                await Task.Delay(300);
                time_out_count++;
            }

            await File.WriteAllTextAsync(FilePath, data, Encoding.UTF8);
            return FilePath;
        }
        /// <summary>
        /// Load data from the file
        /// </summary>
        /// <param name="license">license file</param>
        /// <param name="file">file with license</param>
        /// <param name="secret">secret row for discover license</param>
        public static void LoadFromFile([NotNull] this SessionLicense license, [NotNull] FileInfo file, [NotNull] string secret)
            => license.LoadFromFileAsync(file, secret).Wait();
        /// <summary>
        /// Load data from the file
        /// </summary>
        /// <param name="license">license file</param>
        /// <param name="file">file with license</param>
        /// <param name="secret">secret row for discover license</param>
        public static async Task LoadFromFileAsync([NotNull] this SessionLicense license, [NotNull] FileInfo file, [NotNull] string secret)
        {
            try
            {
                if (!file.Exists)
                    throw new FileNotFoundException(file.FullName, "License file not found");

                if (secret is null)
                    throw new ArgumentNullException(nameof(secret), "Secret row can't be null");

                var time_out_count = 0;
                while (file.IsLocked() && time_out_count < 100)
                {
                    await Task.Delay(300);
                    time_out_count++;
                }

                var lic = (await File.ReadAllTextAsync(file.FullName, Encoding.UTF8))
                   .Decrypt<SessionLicense>(true, secret);
                license.License = lic.License;
                license.Sessions = lic.Sessions;
            }
            catch (FormatException e)
            {
                throw new SessionLicenseExceptions("Invalid format string", nameof(LoadFromFileAsync), e);
            }
            catch (CryptographicException e)
            {
                throw new SessionLicenseExceptions("Invalid cover string", nameof(LoadFromFileAsync), e);
            }
        }

    }

}
