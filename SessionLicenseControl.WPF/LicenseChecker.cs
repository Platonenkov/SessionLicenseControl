using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MathCore.Annotations;
using SessionLicenseControl.WPF.Styles;

namespace SessionLicenseControl.WPF
{
    public sealed class LicenseChecker
    {
        private readonly string _Str;
        private static Expression<Func<string, string>> __CheckStatusExpression = s => s;
        private static Func<string, string> __CheckStatusFunction;

        public static Expression<Func<string, string>> Checker
        {
            get => __CheckStatusExpression;
            set
            {
                if (Equals(__CheckStatusExpression, value)) return;
                __CheckStatusExpression = value ?? (s => s);
                __CheckStatusFunction = __CheckStatusExpression.Compile();
            }
        }
        public static Action OnLicenseLoaded;
        private static SessionLicenseController __LicenseController;
        public static SessionLicenseController LicenseController
        {
            get => __LicenseController;
            set
            {
                __LicenseController = value;
                OnLicenseLoaded?.Invoke();
            }
        }

        private LicenseChecker(string str) => _Str = str;

        public override string ToString() => __CheckStatusFunction(_Str);

        public static implicit operator LicenseChecker(string str) => new(str);

        public static implicit operator string(LicenseChecker status) => status.ToString();
        /// <summary>Проверить статус модели</summary>
        /// <param name="Initialized">Проверка осуществляется в режиме инициализации модели?</param>
        /// <returns>Объект проверки статуса модели</returns>
        [NotNull]
        private static LicenseChecker CheckStatus(bool? Initialized = null)
        {
            if (Initialized is null) return "Ready.";
            if (!Initialized.Value)
                new Task(
                    () =>
                    {
                        Task.Delay(3000);
                        Initialized = string.IsNullOrEmpty(CheckStatus());
                    }).Start();

            return "Initializing...";
        }

        public static void CheckLicense(string FilePath, string Secret, bool NeedStartNewSession, [CanBeNull] string UserName)
        {
            ModernWindow.OnLicenseLoaded += () =>
            {
                LicenseController = ModernWindow.LicenseController;
            };

            Checker = status => ModernWindow.LoadStyle(status, "license.lic", "testwpf", true, "admin");
            CheckStatus(Initialized: false); // Не удалять и не менять положение! Инициализация механизма проверки лицензии

        }
    }
}
