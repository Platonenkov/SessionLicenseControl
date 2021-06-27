using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SessionLicenseControl.WPF
{
    public sealed class StatusChecker
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

        public StatusChecker(string str) => _Str = str;

        public override string ToString() => __CheckStatusFunction(_Str);

        public static implicit operator StatusChecker(string str) => new(str);

        public static implicit operator string(StatusChecker status) => status.ToString();
        /// <summary>Проверить статус модели</summary>
        /// <param name="Initialized">Проверка осуществляется в режиме инициализации модели?</param>
        /// <returns>Объект проверки статуса модели</returns>
        [MathCore.Annotations.NotNull]
        public static StatusChecker CheckStatus(bool? Initialized = null)
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

    }
}
