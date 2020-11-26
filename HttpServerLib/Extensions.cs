using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace  Vlix.HttpServer
{
    public static partial class Extensions
    {
        public static bool EndsWith(this string value, char charToCompare)
        {
            if (value.Length == 0) return false;
            else return value[value.Length - 1] == charToCompare;
        }


        public static int ToInt(this object value, int ValueIfFail = 0)
        {
            if (value == null) return ValueIfFail;
            try
            {
                return Convert.ToInt32(value);
            }
            catch
            {
                return ValueIfFail;
            }
        }

        public static bool ToBool(this object value, bool ValueIfError = false, bool? ValueIfNotAString = null)
        {
            try
            {
                if (value is string S)
                {
                    if (String.IsNullOrWhiteSpace(S)) return ValueIfError;
                    string SL = S.ToLower();
                    if (S == "0" || SL == "false" || SL == "off" || SL == "close" || SL == "no") return false;
                    if (S == "1" || SL == "true" || SL == "on" || SL == "open" || SL == "yes") return true;
                    return ValueIfError;
                }
                else
                {
                    if (ValueIfNotAString == null) return Convert.ToBoolean(value);
                    else return ValueIfNotAString ?? false;
                }
            }
            catch { return ValueIfError; }
        }



        public static string ToFirstRegex(this string StrIn, string Pattern, RegexOptions regexOptions = RegexOptions.IgnoreCase)
        {
            try
            {
                MatchCollection MC = Regex.Matches(StrIn, Pattern, regexOptions);
                if (MC.Count > 0) return MC[0].Value;
                else return "";
            }
            catch
            {
                return "";
            }
        }


        public static bool RegexMatchExist(this string StrIn, string Pattern, RegexOptions regexOptions = RegexOptions.IgnoreCase)
        {
            try
            {
                MatchCollection MC = Regex.Matches(StrIn, Pattern, regexOptions);
                if (MC.Count > 0) return true;
                else return false;
            }
            catch
            {
                return false;
            }
        }
        public static bool IsRegexMatch(this string StrIn, string Pattern, RegexOptions regexOptions = RegexOptions.IgnoreCase)
        {
            try
            {
                if (Pattern == null) return true;
                return Regex.IsMatch(StrIn, Pattern);
            }
            catch
            {
                return false;
            }
        }

    }
}
