using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MathCore.Annotations;
using SessionLicenseControl.Exceptions;
using SessionLicenseControl.Interfaces;
using SessionLicenseControl.Licenses;
using SessionLicenseControl.Session;

namespace SessionLicenseControl
{
    public class LicenseWithSessions : ILicense, ISession
    {
        /// <summary> Sessions data in license </summary>
        public List<DaySessions> Sessions { get; set; } = new();
        #region Implementation of ILicense
        /// <summary> HDD id code </summary>
        public string HDDid { get; set; }
        /// <summary> License expiration time </summary>
        public DateTime? ExpirationDate { get; set; }

        #endregion

        public bool IsValid => ValidateLicense();

        public LicenseWithSessions()
        {

        }
        public LicenseWithSessions(License license)
        {
            ExpirationDate = license.ExpirationDate;
            HDDid = license.HDDid;
        }
        public LicenseWithSessions(string hdd, DateTime? expirationDate)
        {
            ExpirationDate = expirationDate;
            HDDid = hdd;
        }
        public LicenseWithSessions(string row, string secret)
        {
            var license = Decrypt(row, secret);
            Sessions = license.Sessions;
            ExpirationDate = license.ExpirationDate;
            HDDid = license.HDDid;
        }

        public LicenseWithSessions(FileInfo file, string secret) => this.LoadFromFile(file, secret);

        #region Cryptography

        public bool ValidateLicense()=> ValidateLicenseForThisPC() && ValidateSessions();

        public string Encrypt(string Secret) => this.Encrypt(true, Secret);
        public LicenseWithSessions Decrypt(string row, string Secret) => row.Decrypt<LicenseWithSessions>(true, Secret);

        #endregion

        #region Validation and Checks

        #region Validate license

        /// <summary>
        /// Validate license for this pc by hdd 'C'
        /// </summary>
        /// <returns>validation result</returns>
        private bool ValidateLicenseForThisPC() => License.ValidateLicense(new License(HDDid,ExpirationDate), License.GetThisPcHddSerialNumber());

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
            if (ExpirationDate is null)
                return true;

            var trial_days = (ExpirationDate - start_day.Date).Value.Days;
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
        public static string SaveToFile([NotNull] this LicenseWithSessions lic, string FilePath, string secret) => lic.SaveToFileAsync(FilePath, secret).Result;

        /// <summary>
        /// Save data to the file
        /// </summary>
        /// <param name="lic">license file</param>
        /// <param name="FilePath">path where file will be save</param>
        /// <param name="secret">secret row to cover license</param>
        /// <returns>path where file was saved</returns>
        public static async Task<string> SaveToFileAsync([NotNull] this LicenseWithSessions lic, [NotNull] string FilePath, string secret)
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
        /// <param name="LicenseWithSessions">license file</param>
        /// <param name="file">file with license</param>
        /// <param name="secret">secret row for discover license</param>
        public static void LoadFromFile([NotNull] this LicenseWithSessions LicenseWithSessions, [NotNull] FileInfo file, [NotNull] string secret)
            => LicenseWithSessions.LoadFromFileAsync(file, secret).Wait();
        /// <summary>
        /// Load data from the file
        /// </summary>
        /// <param name="LicenseWithSessions">license file</param>
        /// <param name="file">file with license</param>
        /// <param name="secret">secret row for discover license</param>
        public static async Task LoadFromFileAsync([NotNull] this LicenseWithSessions LicenseWithSessions, [NotNull] FileInfo file, [NotNull] string secret)
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
                   .Decrypt<LicenseWithSessions>(true, secret);
                LicenseWithSessions.ExpirationDate = lic.ExpirationDate;
                LicenseWithSessions.HDDid = lic.HDDid;
                LicenseWithSessions.Sessions = lic.Sessions;
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
