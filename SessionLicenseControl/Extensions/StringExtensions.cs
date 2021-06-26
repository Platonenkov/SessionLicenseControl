using System.Text.Json;
using SessionLicenseControl.Information;

namespace System
{
    internal static class StringExtensions
    {
        /// <summary> Create string data with session info </summary>
        /// <param name="data">string data</param>
        /// <param name="Secret">secret string if you want to encrypt data</param>
        /// <returns></returns>
        public static string EncryptToRow<T>(this T data, string Secret)
        {
            var json = JsonSerializer.Serialize(data);
            return Secret is not null ? json.Cover(Secret) : json;
        }

        /// <summary>
        /// Get data from string
        /// </summary>
        /// <param name="data">string data</param>
        /// <param name="Secret">secret string if data was encrypted</param>
        /// <returns></returns>
        public static T DecryptRow<T>(this string data, string Secret)
        {
            var options = new JsonSerializerOptions()
            {
                AllowTrailingCommas = true,
                IgnoreReadOnlyFields = true,
                IgnoreReadOnlyProperties = true,
                IncludeFields = true
            };
            return JsonSerializer.Deserialize<T>(Secret is not null ? data.Discover(Secret) : data, options);
        }
    }
}
