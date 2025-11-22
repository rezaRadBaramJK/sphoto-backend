using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Plugin.Baramjk.Framework.Data;

namespace Nop.Plugin.Baramjk.Framework.Extensions
{
    public static class StringEx
    {
        public static bool IsEmptyOrNull(this string str) => string.IsNullOrEmpty(str);
        public static bool HasValue(this string str) => string.IsNullOrEmpty(str) == false;

        public static string ValueOrDefault(this string str, string defaultValue) =>
            string.IsNullOrEmpty(str) ? defaultValue : str;

        public static List<string> SplitSafe(this string str, string separator = ",", bool trim = true)
        {
            if (string.IsNullOrEmpty(str))
                return FrameworkDefaultValues.EmptyStringList;

            if (trim)
                return str.Split(separator).Select(item => item.Trim()).ToList();
            else
                return str.Split(separator).ToList();
        }

        public static string ReplaceSafe(this string value, string oldValue, string newValue, string defaultValue = "")
        {
            if (string.IsNullOrEmpty(value))
                return defaultValue;

            return value.Replace(oldValue, newValue);
        }

        public static string TrimSafe(this string value, string defaultValue = "")
        {
            if (string.IsNullOrEmpty(value))
                return defaultValue;

            return value.Trim();
        }

        public static bool ContainsSafe(this string value, string keyword)
        {
            if (value == null)
                return false;

            return value.Contains(keyword);
        }

        public static string Plus(this string value, string plus, bool addNewLine = true)
        {
            if ((addNewLine && value != null))
                plus = $"{Environment.NewLine}{plus}";

            return $"{value ?? ""}{plus}";
        }

        public static string ExtractFirstNumber(this string str, string defaultValue = "0", bool includeDot = true)
        {
            if (string.IsNullOrEmpty(str))
                return defaultValue;

            var result = string.Join("", str
                .SkipWhile(x => !char.IsDigit(x))
                .TakeWhile(x => char.IsDigit(x) || (includeDot && x == '.')));

            if (string.IsNullOrEmpty(result))
                return result;

            return result;
        }

        public static decimal ExtractFirstDecimal(this string str, decimal defaultValue = 0)
        {
            if (string.IsNullOrEmpty(str))
                return defaultValue;
            var result = ExtractFirstNumber(str);
            if (string.IsNullOrEmpty(result))
                return defaultValue;
            return decimal.Parse(result);
        }

        public static int ExtractFirstInteger(this string str, int defaultValue = 0)
        {
            if (string.IsNullOrEmpty(str))
                return defaultValue;

            var result = ExtractFirstNumber(str, includeDot: false);
            if (string.IsNullOrEmpty(result))
                return defaultValue;

            return int.Parse(result);
        }
    }
}