//Resharper disable all

using System;
using UnityEngine;

namespace Library.Extensions{
    public static class DateTimeExtension{
        public static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0);

        public static long ToUnixTime(this DateTime dateTime) {
            var t = dateTime - epoch;
            return (long) t.TotalSeconds;
        }

        public static DateTime FromUnixTime(long unixTime) {
            return epoch.AddSeconds(unixTime);
        }

        public static DateTime FromUnixTime(string unixTime, DateTime defaultTime) {
            try {
                long t;
                if (long.TryParse(unixTime, out t))
                    return FromUnixTime(t);
            } catch (Exception e) {
                Debug.LogException(e);
            }

            return defaultTime;
        }
    }
}