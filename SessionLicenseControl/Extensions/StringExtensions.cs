using System.Text.Json;
using Newtonsoft.Json;
using SessionLicenseControl.Information;

namespace System
{
    internal static class StringExtensions
    {
        /// <summary> Create string data with session info </summary>
        /// <param name="data">string data</param>
        /// <param name="NeedCover">Need cover data</param>
        /// <param name="CoverRow">cover row for data</param>
        /// <returns></returns>
        public static string EncryptToRow<T>(this T data, bool NeedCover, string CoverRow)
        {
            var json = JsonConvert.SerializeObject(data);
            return NeedCover ? json.Cover(CoverRow) : json;
        }

        /// <summary>
        /// Get data from string
        /// </summary>
        /// <param name="data">string data</param>
        /// <param name="NeedDiscover">Need discover data</param>
        /// <param name="CoverRow">cover row for data</param>
        /// <returns></returns>
        public static T DecryptRow<T>(this string data, bool NeedDiscover, string CoverRow)
        {
            if(!NeedDiscover)
                return JsonConvert.DeserializeObject<T>(data);

            var row = data.Discover(CoverRow);
            if (row.IsNullOrWhiteSpace())
                return default;
            var options = new JsonSerializerOptions()
            {
                AllowTrailingCommas = true,
                IgnoreReadOnlyFields = true,
                IgnoreReadOnlyProperties = true,
                IncludeFields = true
            };
            var s = JsonConvert.DeserializeObject<T>(row,new JsonSerializerSettings() );//TODO НЕ ЧИТАЕТ СПИСОК СЕССИЙ
            return row.StartsWith("[") && row.EndsWith("]") || row.StartsWith("{") && row.EndsWith("}") 
                ? JsonConvert.DeserializeObject<T>(row)
                : default;
        }
    }
}
