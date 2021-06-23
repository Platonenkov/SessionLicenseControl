﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SessionLicenseControl.Information;
using SessionLicenseControl.Json;
using SessionLicenseControl.Session;

namespace System
{
    public static class StringExtensions
    {
        /// <summary> Create string data with session info </summary>
        /// <param name="data">string data</param>
        /// <param name="NeedCover">Need cover data</param>
        /// <param name="CoverRow">cover row for data</param>
        /// <returns></returns>
        public static string CreateDataRow<T>(this T data, bool NeedCover, string CoverRow)
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
        public static T GetDataFromRow<T>(this string data, bool NeedDiscover, string CoverRow)
        {
            if(!NeedDiscover)
                return JsonConvert.DeserializeObject<T>(data);

            var row = data.Discover(CoverRow);
            if (row.IsNullOrWhiteSpace())
                return default;

            return row.StartsWith("[") && row.EndsWith("]") || row.StartsWith("{") && row.EndsWith("}") 
                ? JsonConvert.DeserializeObject<T>(row)
                : default;


            //var section = json_data?.FirstOrDefault(d => d.Name == SectionName);
            //if (section is null)
            //    return default;
            //var sessions = JsonConvert.DeserializeObject<T>(section);
            //return sessions;
        }

    }
}
