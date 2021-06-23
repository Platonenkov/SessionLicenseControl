//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Text.Json.Serialization;
//using System.Threading.Tasks;
//using SessionLicenseControl.Information;

//namespace SessionLicenseControl.Licenses
//{
//    [Serializable]
//    public class TrialTimeWatcher
//    {
//        [JsonIgnore]
//        public static string FilePath = Path.Combine(Environment.CurrentDirectory, "LicenseWatcher.lic");
//        public TrialTimeWatcher()
//        {

//        }

//        public TrialTimeWatcher(string path) => Load(path);

//        public TrialTimeWatcher(int days)
//        {
//            TrialDays = days;
//            Save(FilePath);
//        }
//        public TrialTimeWatcher(DateTime date)
//        {
//            var days = (date - DateTime.Now).Days + 1;
//            if (days <= 0)
//                throw new ArgumentOutOfRangeException(
//                    $"Дата лицензии устарела, разрешённая дата: \n{date.ToShortDateString()}\n Обратитесь к держателю лицензии за новой");

//            TrialDays = days;
//            WorkDayList.Add(new TrialDayWatcher(DateTime.Now));

//            Save(FilePath);
//        }

//        private static int MaxTrialDays = 60;

//        /// <summary>
//        /// Число отпущенных дней
//        /// </summary>
//        public int TrialDays;

//        /// <summary> Дата начала работы программы </summary>
//        public DateTime StartDate { get; set; } = DateTime.Now;
//        /// <summary> Список отработанных ранее дней </summary>
//        public List<TrialDayWatcher> WorkDayList { get; set; } = new();

//        /// <summary> Получает количество отработанных дней </summary>
//        /// <returns>число дней когда программа работала</returns>
//        public int GetFulfilledDaysCount() => WorkDayList.Count;


//        /// <summary> Выводит сообщения окончания лицензии </summary>
//        private void SendErrorMessag() => throw
//            new FileNotFoundException("Time ended");

//        #region Save/Load
//        /// <summary>
//        /// Сохранение лицензии в файл
//        /// </summary>
//        /// <param name="path">Путь к файлу</param>
//        public void Save(string path)
//        {
//            try
//            {
//                var json = Newtonsoft.Json.JsonConvert.SerializeObject(this);
//                var cover_code = json.Cover("RRJExpress");
//                File.WriteAllText(path, cover_code);

//            }
//            catch (Exception e)
//            {
//                Trace.WriteLine($"TRIAL SAVE ERROR: {e.Message}");
//                throw;
//            }
//        }
//        /// <summary>
//        /// Загрузка лицензии из файла
//        /// </summary>
//        /// <param name="path">Путь к файлу</param>
//        public void Load(string path)
//        {
//            if (!File.Exists(path))
//            {
//                //MessageBox.Show("Incorrect License for this version", "License error", MessageBoxButton.OK, MessageBoxImage.Warning);
//                throw new FileNotFoundException("License file not found");
//            }
//            try
//            {
//                var file = new FileInfo(path);
//                //TrialTimeWatcher watcher;
//                var time_out_count = 0;
//                while (file.IsLocked() && time_out_count < 100)
//                {
//                    Task.Delay(300);
//                    time_out_count++;
//                }

//                var license_text = File.ReadAllText(path, Encoding.UTF8);
//                var watcher = GetTrialDataFromString(license_text) ?? new TrialTimeWatcher();
//                StartDate = watcher.StartDate;
//                WorkDayList = watcher.WorkDayList;
//                TrialDays = watcher.TrialDays;
//                if (StartDate == default)
//                    StartDate = DateTime.Now;
//                if (WorkDayList.Contains(d => d.Day.Date > DateTime.Now.Date))
//                    SendErrorMessag();

//                AddSession();

//                //#if DEBUG
//                //                return; 
//                //#endif
//                CheckLicense();

//                Save(path);
//            }
//            catch (Exception e)
//            {
//                Trace.WriteLine($"TRIAL LOAD ERROR: {e.Message}");
//                throw;
//            }
//        }

//        public static TrialTimeWatcher GetTrialTimeWatcher() => GetTrialTimeWatcher(TrialTimeWatcher.FilePath);

//        public static TrialTimeWatcher GetTrialTimeWatcher(string LicensePath)
//        {
//            if (!File.Exists(LicensePath))
//                return null;
//            var license_text = File.ReadAllText(LicensePath, Encoding.UTF8);
//            return GetTrialDataFromString(license_text);
//        }
//        private void AddSession()
//        {
//            var session_start_time = DateTime.Now;
//            var current_day_watcher = WorkDayList.FirstOrDefault(d => d.Day.Date == session_start_time.Date);

//            if (current_day_watcher != null)
//            {
//                current_day_watcher.StartNewSession(session_start_time);
//            }
//            else
//            {
//                WorkDayList.Add(new TrialDayWatcher(session_start_time));
//            }
//        }
//        /// <summary>
//        /// Получает объект данных триал версии
//        /// </summary>
//        /// <param name="data">строка данных из файла с зашифрованными данными</param>
//        /// <returns>сериализованный объект данных</returns>
//        private static TrialTimeWatcher GetTrialDataFromString(string data)
//        {
//            if (data.IsNullOrWhiteSpace()) return null;
//            var ls = data.Discover("RRJExpress");
//            return !ls.StartsWith("{") || !ls.EndsWith("}") ? null
//                : Newtonsoft.Json.JsonConvert.DeserializeObject<TrialTimeWatcher>(ls);
//        }

//        #endregion

//        #region Проверка лицензии

//        /// <summary>
//        /// Проверка лицензии
//        /// </summary>
//        private void CheckLicense()
//        {
//            if (StartDate > DateTime.Now)
//                SendErrorMessag();
//            if (DateTime.Now > StartDate + TimeSpan.FromDays(TrialDays))
//                SendErrorMessag();

//            CheckTrial();
//            CheckTrialPeriod(TrialDays);
//        }
//        /// <summary> Проверяет количество рабочих дней и выводит сообщение если превышено </summary>
//        public void CheckTrial()
//        {
//            var days = TrialDays - GetFulfilledDaysCount();
//            if (days < 0)
//                SendErrorMessag();
//        }

//        /// <summary> Проверяет количество рабочих дней и выводит сообщение если превышено </summary>
//        /// <param name="TrialDaysCount">Количество дней Trial периода</param>
//        public void CheckTrialPeriod(int TrialDaysCount)
//        {
//            //if (TrialDaysCount > MaxTrialDays) throw new ArgumentOutOfRangeException(nameof(TrialDaysCount));
//            var first_session = WorkDayList.First().Day.Sessions.First().StartTime.Date;
//            if (first_session.Date != StartDate.Date) SendErrorMessag();
//            var total = DateTime.Now - first_session;
//            var total_days = total.Days;
//            if (total_days > TrialDaysCount)
//                SendErrorMessag();
//        }

//        #endregion

//    }
//}
