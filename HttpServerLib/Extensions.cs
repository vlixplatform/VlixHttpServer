using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Vlix
{
    public static partial class Extensions
    {
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
    }
}
