using System;
using MathCore.Annotations;
using DST = System.Diagnostics.DebuggerStepThroughAttribute;

namespace SessionLicenseControl.WPF.Extensions
{
    /// <summary>Класс методов-расширений класса <see cref="object"/></summary>
    public static class ObjectEx
    {
        /// <summary>Инициализация объекта</summary>
        /// <typeparam name="T">Тип объекта</typeparam>
        /// <param name="obj">Инициализируемый объект</param>
        /// <param name="Initializer">Метод инициализации объекта</param>
        /// <returns>Исходный объект</returns>
        [DST]
        public static T Init<T>([CanBeNull] this T obj, [NotNull] Action<T> Initializer)
        {
            if (obj != null) Initializer(obj);
            return obj;
        }

    }
}
