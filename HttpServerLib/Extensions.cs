using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Diagnostics;
using System.Xml.Serialization;
using System.Data;
using System.Xml;
using System.Xml.Linq;
using System.Collections;
using System.Globalization;
using Microsoft.Win32;
using System.Runtime.Serialization.Formatters.Binary;
using System.ComponentModel;
using System.Threading;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Security.Cryptography;
using System.Collections.Concurrent;
using System.Net;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace Vlix.HttpServer
{
    public static partial class Extensions
    {

        public static async Task<T> WaitOrCancel<T>(this Task<T> task, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            await Task.WhenAny(task, token.WhenCanceled());
            token.ThrowIfCancellationRequested();

            return await task;
        }

        public static Task WhenCanceled(this CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), tcs);
            return tcs.Task;
        }

        public static bool IsDateTimeType(this object o)
        {
            if (o == null) return false;
            return (o.GetType() == typeof(DateTime));
        }

        public static bool IsNumericType(this object o)
        {
            if (o == null) return false;
            switch (Type.GetTypeCode(o.GetType()))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsIntType(this object o)
        {
            if (o == null) return false;
            switch (Type.GetTypeCode(o.GetType()))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    return true;
                default:
                    return false;
            }
        }
        public static bool IsRealType(this object o)
        {
            if (o == null) return false;
            switch (Type.GetTypeCode(o.GetType()))
            {
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        public static string RemoveLastCharacter(this object Object, char RemoveIfCharacterIs = Char.MinValue)
        {
            if (Object is String Text)
            {
                if (Text.Length == 0) return Text;
                if (RemoveIfCharacterIs == Char.MinValue)
                {
                    return Text.Substring(0, Text.Length - 1);
                }
                else
                {
                    if (Text.Last() == RemoveIfCharacterIs)
                    {
                        return Text.Substring(0, Text.Length - 1);
                    }
                }
            }
            return Object.ToString();
        }

        public static string RemoveDBLIllegalCharacters(this string StrIn)
        {
            if (StrIn.Contains("Last Custom Stuff"))
            {
            }
            string StrOut = StrIn?.Replace("\r", "").Replace("\n", "");
            return StrOut;
        }


        public static IList RemoveLastElement(this IList list)
        {
            int LC = list.Count;
            if (LC > 0) list.RemoveAt(LC - 1);
            return list;
        }



        public static IList RemoveFirstElement(this IList list)
        {
            int LC = list.Count;
            if (LC > 0) list.RemoveAt(0);
            return list;
        }

        public static List<string> ToList_FromCSVString(this string CSVStr)
        {
            List<string> CSVArray = CSVStr.Split(',').ToList();
            int CC = CSVArray.Count();
            if (CC > 0 && CSVArray.Last().IsNullOrWhiteSpace()) CSVArray.RemoveLastElement();
            return CSVArray;
        }

        public static string[] ToStringArray_FromCSVString(this string CSVStr)
        {
            string[] CSVArray = CSVStr.Split(',');
            int CC = CSVArray.Count();
            if (CC > 0 && CSVArray.Last().IsNullOrWhiteSpace()) CSVArray.RemoveLastElement();
            return CSVArray;
        }


        //public static string[] RemoveLastElement(this string[] list)
        //{
        //    int LC = list.Count();
        //    if (LC > 0) list.RemoveAt(LC - 1);
        //    return list;
        //}

        public static string RemoveAllNewLines(this string MultipleLinesText)
        {
            MultipleLinesText = MultipleLinesText?.Replace('\r', ' ');
            MultipleLinesText = MultipleLinesText?.Replace("\n", "");
            return MultipleLinesText;
        }

        public static string ToECMADateTimeString_Local(this DateTime DT)
        {
            return DT.ToString("yyyy-MM-ddTHH:mm:ss.fff");
        }
        public static string ToECMADateTimeString(this DateTime DT)
        {
            return DT.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        }

        public static string ToPasswordMask(this string PW)
        {
            return "".PadRight(PW.Length, '*');
        }

        public static List<List<TSource>> GroupByWithGroupSizeLimit<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, int GroupSizeLimit)
        {
            List<List<TSource>> ResultGroups = new List<List<TSource>>();
            IEnumerable<IGrouping<TKey, TSource>> RawGroups = source.GroupBy(keySelector);
            if (GroupSizeLimit == 0)
            {
                foreach (IGrouping<TKey, TSource> RawGroup in RawGroups) ResultGroups.Add(RawGroup.ToList());
            }
            else
            {
                IEnumerable<IGrouping<TKey, TSource>> UpdateGroupsLessThanLimit = RawGroups.Where(G => G.Count() <= GroupSizeLimit);
                foreach (IGrouping<TKey, TSource> GroupLessThanLimit in UpdateGroupsLessThanLimit) ResultGroups.Add(GroupLessThanLimit.ToList());
                IEnumerable<IGrouping<TKey, TSource>> GroupsLargerThanLimit = RawGroups.Where(G => G.Count() > GroupSizeLimit);
                foreach (IGrouping<TKey, TSource> GroupLargerThanLimit in GroupsLargerThanLimit)
                {
                    List<TSource> SplitGroup = new List<TSource>();
                    foreach (TSource Item in GroupLargerThanLimit)
                    {
                        SplitGroup.Add(Item);
                        if (SplitGroup.Count >= GroupSizeLimit)
                        {
                            ResultGroups.Add(SplitGroup);
                            SplitGroup = new List<TSource>();
                        }
                    }
                    if (SplitGroup.Count > 0) ResultGroups.Add(SplitGroup);
                }
            }
            return ResultGroups;
        }


        public static void Remove(this ConcurrentDictionary<string, object> value, string PName)
        {
            if (!value.TryRemove(PName, out object Val))
            {
            }
        }

        public static string GetLastSyntax(this string Value)
        {
            if (string.IsNullOrEmpty(Value)) return "";
            //return Regex.Match(Value, @"([^\s^\(^\{^\[^\+^\-^\)^\}^\)^\=^\>^\<^\|^\*^\.^\!^\,])*\.$").Value;
            return Regex.Match(Value, @"([^\s^\(^\{^\[^\+^\-^\)^\}^\)^\=^\>^\<^\|^\*^\.^\!^\,])*(?=\.$)").Value;
        }
        public static bool In<T>(this T obj, params T[] args)
        {
            return args.Contains(obj);
        }

        public static byte[] ToByteArray(this BitArray bits)
        {
            byte[] ret = new byte[(bits.Length - 1) / 8 + 1];
            bits.CopyTo(ret, 0);
            return ret;
        }

        public static string ToTwoDecimalPlacesForDisplay(this double Value)
        {
            return Value.ToString("0.00");
        }
        public static string ToTwoDecimalPlacesForDisplay(this Single Value)
        {
            return Value.ToString("0.00");
        }
        public static string ToTwoDecimalPlacesForDisplay(this Decimal Value)
        {
            return Value.ToString("0.00");
        }


        public static string ToMySqlTimeStamp(this DateTime Value)
        {
            //return Value.ToString("yyyy-MM-dd HH:mm:ss.fff");
            return Value.ToString("yyyy-MM-dd HH:mm:ss.fff");
            //return Value.Year + "-" + Value.Month + '-' + Value.Day + " " + Value.Hour + ":" + Value.Minute + ":" + Value.Second + "." + Value.Millisecond;
        }


        public static string ToECMAISO8601_String(this DateTime Value)
        {
            //return Value.ToString("o");
            return Value.ToString("yyyy-MM-dd'T'HH:mm:ss.fffK");

        }

        public static bool IsNumeric(this string Str)
        {
            if (double.TryParse(Str, out double num))
            {
                return true;
            }
            return false;
        }

        [DebuggerStepThrough]
        public static bool IsFullPath(this string FileName)
        {
            return (Path.IsPathRooted(FileName));
        }


        //public static IComparable ToOPStandardIntRealBool(this object OPData, out Interpolation InterpType, out DataType DataType)
        //{
        //    InterpType = Interpolation.Linear;
        //    DataType = DataType.Real;
        //    try
        //    {
        //        //Insert Data into RT Cache
        //        string ResultType = OPData.GetType().Name.ToLower();
        //        switch (ResultType)
        //        {
        //            case "bool":
        //                InterpType = Interpolation.Digital;
        //                DataType = DataType.Bool;
        //                return (bool)OPData;
        //            case "int": case "integer": case "int32": case "int16": case "int64": case "intptr": case "uint": case "uint16": case "uint32": case "uint64": case "uintptr": case "sbyte": case "short": case "ulong": case "ushort":
        //                InterpType = Interpolation.Digital;
        //                DataType = DataType.Int;
        //                return OPData.ToInt();
        //            case "double": case "float": case "single": case "decimal":
        //                InterpType = Interpolation.Linear;
        //                DataType = DataType.Real;
        //                return OPData.ToDouble();
        //        }
        //        return null;
        //    }
        //    catch { return null; }
        //}

        public static int ToInt16(this BitArray BitArray)
        {
            if (BitArray.Length < 16)
                throw new ArgumentException("Argument length shall be at most 32 bits.");

            Int16[] array = new Int16[1];
            BitArray.CopyTo(array, 0);
            return array[0];

        }

        public static int ToInt32(this BitArray bitArray)
        {

            if (bitArray.Length > 32)
                throw new ArgumentException("Argument length shall be at most 32 bits.");

            int[] array = new int[1];
            bitArray.CopyTo(array, 0);
            return array[0];

        }

        public static void RemoveAll(this IList list)
        {
            while (list.Count > 0)
            {
                list.RemoveAt(list.Count - 1);
            }
        }

        public static T[] RemoveAt<T>(this T[] source, int index)
        {
            T[] dest = new T[source.Length - 1];
            if (index > 0)
                Array.Copy(source, 0, dest, 0, index);

            if (index < source.Length - 1)
                Array.Copy(source, index + 1, dest, index, source.Length - index - 1);

            return dest;
        }

        [DebuggerStepThrough]
        public static string UppercaseFirstLetter(this string value)
        {
            if (value.Length > 0)
            {
                char[] array = value.ToCharArray();
                array[0] = char.ToUpper(array[0]);
                return new string(array);
            }
            return value;
        }

        [DebuggerStepThrough]
        public static string ToSha1(this string value)
        {
            var message = Encoding.ASCII.GetBytes(value);
            SHA1Managed hashString = new SHA1Managed();
            string hex = "";

            var hashValue = hashString.ComputeHash(message);
            foreach (byte x in hashValue)
            {
                hex += String.Format("{0:x2}", x);
            }
            return hex;
        }

        [DebuggerStepThrough]
        public static string ToSha256(this string value)
        {
            var message = Encoding.ASCII.GetBytes(value);
            SHA256Managed hashString = new SHA256Managed();
            string hex = "";

            var hashValue = hashString.ComputeHash(message);
            foreach (byte x in hashValue)
            {
                hex += String.Format("{0:x2}", x);
            }
            return hex;
        }

        [DebuggerStepThrough]
        public static string Crypt(this string text)
        {
            if (text == null) return null;
            //encryption   
            string output = "";
            char[] readChar = text.ToCharArray();
            for (int i = 0; i < readChar.Length; i++)
            {
                int no = Convert.ToInt32(readChar[i]) + 10;
                string r = Convert.ToChar(no).ToString();
                output += r;
            }
            return output;
        }


        [DebuggerStepThrough]
        public static string Decrypt(this string text)
        {
            if (text == null) return null;
            //decryption  
            string output = "";
            char[] readChar = text.ToCharArray();
            for (int i = 0; i < readChar.Length; i++)
            {
                int no = Convert.ToInt32(readChar[i]) - 10;
                string r = Convert.ToChar(no).ToString();
                output += r;
            }
            return output;
        }

        [DebuggerStepThrough]
        public static string ToMD5(this string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

        [DebuggerStepThrough]
        public static bool IsNullOrWhiteSpace(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        [DebuggerStepThrough]
        public static bool IsEmptyOrWhiteSpace(this string value) => value.All(char.IsWhiteSpace);


        [DebuggerStepThrough]
        public static string Right(this string value, int length)
        {
            return value.Substring(value.Length - length);
        }

        [DebuggerStepThrough]
        public static SortedList<TKey, TValue> ToSortedList<TSource, TKey, TValue>
        (this IEnumerable<TSource> source,
         Func<TSource, TKey> keySelector,
         Func<TSource, TValue> valueSelector)
        {
            // TODO: Argument validation
            var ret = new SortedList<TKey, TValue>();
            foreach (var element in source)
            {
                ret.Add(keySelector(element), valueSelector(element));
            }
            return ret;
        }

        [DebuggerStepThrough]
        public static DateTime FromUnixTime(this long unixTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(unixTime);
        }

        [DebuggerStepThrough]
        public static long ToUnixTime(this DateTime date)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64((date - epoch).TotalSeconds);
        }

        [DebuggerStepThrough]
        public static int ToInt32(this string value, out string ErrorLog)
        {
            int Temp = 0;
            ErrorLog = "";
            try
            {
                Temp = Convert.ToInt32(value);
            }
            catch (Exception ex)
            {
                ErrorLog = "ERROR: Unable to Convert string '" + value + "' to Int32 DataType.\r\n" + ex.ToString();
            }
            return Temp;
        }

        [DebuggerStepThrough]
        public static Single ToSingle(this string value, out string ErrorLog)
        {
            Single Temp = 0;
            ErrorLog = "";
            try
            {
                Temp = Convert.ToSingle(value);
            }
            catch (Exception ex)
            {
                ErrorLog = "ERROR: Unable to Convert string '" + value + "' to Single DataType.\r\n" + ex.ToString();
            }
            return Temp;
        }

        [DebuggerStepThrough]
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

        [DebuggerStepThrough]
        public static void ToTagNameAndParameter(this string TagId, out string TagName, out string Parameter)
        {
            string[] Splitted = TagId.Split('.');
            if (Splitted.Count() == 1)
            {
                TagName = Splitted[0];
                Parameter = "";
            }
            else
            {
                Parameter = Splitted.Last();
                string TN = "";
                if (Splitted.Count() == 2)
                {
                    TN = Splitted[0];
                }
                else
                {
                    for (int n = 0; n < Splitted.Count() - 1; n++)
                    {
                        TN = TN + Splitted[n] + ".";
                    }
                }
                TagName = TN;
            }
        }


        public static int? ToIntNull(this object value)
        {
            if (value == null) return null;
            try
            {
                return Convert.ToInt32(value);
            }
            catch
            {
                return null;
            }
        }


        public static Int16 ToInt16(this object value, Int16 ValueIfError = 0)
        {
            try
            {
                return Convert.ToInt16(value);
            }
            catch
            {
                return ValueIfError;
            }
        }

        public static Int64 ToInt64(this object value, Int64 ValueIfError = 0)
        {
            try
            {
                return Convert.ToInt64(value);
            }
            catch
            {
                return ValueIfError;
            }
        }

        public static uint ToUint(this object value, uint ValueIfError = 0)
        {
            try
            {
                return Convert.ToUInt32(value);
            }
            catch
            {
                return ValueIfError;
            }
        }

        public static UInt16 ToUint16(this object value, UInt16 ValueIfError = 0)
        {
            try
            {
                return Convert.ToUInt16(value);
            }
            catch
            {
                return ValueIfError;
            }
        }

        public static UInt64 ToUint64(this object value, UInt64 ValueIfError = 0)
        {
            try
            {
                return Convert.ToUInt64(value);
            }
            catch
            {
                return ValueIfError;
            }
        }


        /// <summary>
        /// in a Byte swap, every single Word Component, in a Word[] array has it's two bytes swapped
        /// </summary>
        public static ushort[] ByteSwap(this ushort[] Words)
        {
            ushort[] RetVal = new ushort[Words.Count()];
            int n = 0;
            foreach (ushort Word in Words)
            {
                Byte[] BytePair = Word.ToBytes();
                Byte[] BytePairSwapped = new Byte[2];
                BytePairSwapped[0] = BytePair[1];
                BytePairSwapped[1] = BytePair[0];
                RetVal[n] = BitConverter.ToUInt16(BytePairSwapped, 0);
                n++;
            }
            return RetVal;
        }


        /// <summary>
        /// This has been checked to work correctly confirming to IEEE Standards when used with BitConverter Class
        /// </summary>
        public static Byte[] ToBytes(this ushort[] Words)
        {
            Byte[] RetVal = new byte[Words.Count() * 2];
            int n = 0;
            foreach (ushort W in Words)
            {
                Byte[] T = BitConverter.GetBytes(W);
                RetVal[n] = T[0];
                n++;
                RetVal[n] = T[1];
                n++;
            }
            return RetVal;
        }

        public static Byte[] ToBytes(this ushort Word)
        {
            Byte[] RetVal = new byte[2];
            Byte[] T = BitConverter.GetBytes(Word);
            RetVal[0] = T[0];
            RetVal[1] = T[1];
            return RetVal;
        }

        public static ushort[] WordSwap(this ushort[] Words)
        {
            int WC = Words.Count();
            ushort[] WordsReversed = new ushort[Words.Count()];
            for (int n = WC - 1; n >= 0; n--) WordsReversed[WC - n - 1] = Words[n];
            return WordsReversed;
        }


        public static string Compress(this string s)
        {
            var bytes = Encoding.Unicode.GetBytes(s);
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress))
                {
                    msi.CopyTo(gs);
                }
                return Convert.ToBase64String(mso.ToArray());
            }
        }

        public static string Decompress(this string s)
        {
            var bytes = Convert.FromBase64String(s);
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    gs.CopyTo(mso);
                }
                return Encoding.Unicode.GetString(mso.ToArray());
            }
        }


        public static Byte ToByte(this object value)
        {
            try
            {
                //if ((string)value == "1") return true;
                //if ((string)value == "0") return false;
                return Convert.ToByte(value);
            }
            catch { return (Byte)0; }
        }



        public static ushort ToUShort(this object value)
        {
            try
            {
                //if ((string)value == "1") return true;
                //if ((string)value == "0") return false;
                return Convert.ToUInt16(value);
            }
            catch { return (ushort)0; }
        }



        public static Char ToChar(this object value, bool ValueIfError = false)
        {
            try
            {
                //if ((string)value == "1") return true;
                //if ((string)value == "0") return false;
                return Convert.ToChar(value);
            }
            catch { return ' '; }
        }
        public static string To35String(this int value)
        {
            switch (value)
            {
                case 0: return "0";
                case 1: return "1";
                case 2: return "2";
                case 3: return "3";
                case 4: return "4";
                case 5: return "5";
                case 6: return "6";
                case 7: return "7";
                case 8: return "8";
                case 9: return "9";
                case 10: return "A";
                case 11: return "B";
                case 12: return "C";
                case 13: return "D";
                case 14: return "E";
                case 15: return "F";
                case 16: return "G";
                case 17: return "H";
                case 18: return "I";
                case 19: return "J";
                case 20: return "K";
                case 21: return "L";
                case 22: return "M";
                case 23: return "N";
                case 24: return "O";
                case 25: return "P";
                case 26: return "Q";
                case 27: return "R";
                case 28: return "S";
                case 29: return "T";
                case 30: return "U";
                case 31: return "V";
                case 32: return "W";
                case 33: return "X";
                case 34: return "Y";
                case 35: return "Z";
                default: return "A";
            }
        }


        [DebuggerStepThrough]
        public static int ToIntWithErrorLog(this object value, out string ErrorLog)
        {
            int Temp = 0;
            ErrorLog = "";
            try
            {
                Temp = Convert.ToInt32(value);
            }
            catch (Exception ex)
            {
                ErrorLog = "ERROR: Unable to Convert object '" + value.ToString() + "' to int.\r\n" + ex.ToString();
            }
            return Temp;
        }

        [DebuggerStepThrough]
        public static Single ToSingle(this object value, out string ErrorLog)
        {
            Single Temp = 0;
            ErrorLog = "";
            try
            {
                Temp = Convert.ToSingle(value);
            }
            catch (Exception ex)
            {
                ErrorLog = "ERROR: Unable to Convert object '" + value.ToString() + "' to Single DataType.\r\n" + ex.ToString();
            }
            return Temp;
        }

        public static long ToLong(this object value, Int64 DefaultValueIfError = Int64.MinValue)
        {
            long Temp = DefaultValueIfError;
            try
            {
                Temp = Convert.ToInt64(value);
            }
            catch
            {
            }
            return Temp;
        }

        public static void Clear<T>(this ConcurrentQueue<T> queue)
        {
            T item;
            while (queue.TryDequeue(out item))
            {
                // do nothing
            }
        }

        public static string ToXMLString(this object Obj)
        {
            using (var stringwriter = new System.IO.StringWriter())
            {
                var serializer = new XmlSerializer(Obj.GetType());
                serializer.Serialize(stringwriter, Obj);
                return stringwriter.ToString();
            }
        }

        public static TimeSpan ToTimeSpan(this object value)
        {
            TimeSpan Temp = TimeSpan.Zero;
            try
            {
                Temp = (TimeSpan)value;
            }
            catch
            {
            }
            return Temp;
        }


        public static DateTime ToDateTime(this object value, bool IfTimeZoneNotSpecifiedAssumeUTC = true, DateTime? ValueIfFail = null)
        {
            DateTime DT;
            try
            {
                if (value is string TSStr)
                {
                    if (DateTime.TryParseExact(TSStr, new string[] { "yyyy-MM-dd HH:mm:ss.fff", "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd HH:mm", "yyyy-M-d H:m:s", "yyyy-M-d HH:m:s.f" },
                            System.Globalization.CultureInfo.InvariantCulture, IfTimeZoneNotSpecifiedAssumeUTC ? DateTimeStyles.AssumeUniversal : DateTimeStyles.AssumeLocal, out DT))
                    {
                        if (IfTimeZoneNotSpecifiedAssumeUTC) DT = DT.ToUniversalTime();
                    }
                    else DateTime.TryParse(TSStr, null, DateTimeStyles.RoundtripKind, out DT); //This handles ECMA ISO8601 Date Format
                }
                else DT = Convert.ToDateTime(value);
                return DT;
            }
            catch
            {
                return ValueIfFail ?? DateTime.MinValue;
            }
        }
        //public static DateTime ToDateTimeFromISO8601String(this string DTStr)
        //{
        //    DateTime Temp = DateTime.MinValue;
        //    try
        //    {
        //        if (DateTime.TryParse(DTStr, null, DateTimeStyles.RoundtripKind, out DateTime Res)) return Res;
        //    }
        //    catch
        //    {
        //    }
        //    return Temp;
        //}

        public static DateTime[] ToDateTimeUTCArray(this IList<string> List, DateTime? ValueIfFail = null, DateTime? ValueIfEmptyString = null)
        {
            DateTime[] Result = new DateTime[List.Count];
            for (int n = 0; n < List.Count; n++)
            {
                var Item = List[n];
                if (Item.IsNullOrWhiteSpace()) Result[n] = ValueIfEmptyString ?? DateTime.UtcNow;
                else
                {
                    DateTime Temp = Item.ToDateTimeUTC_ParseTimeStampSemantic();
                    if (Temp == DateTime.MinValue) Result[n] = ValueIfFail ?? DateTime.UtcNow; else Result[n] = Temp;
                }
            }
            return Result;
        }



        public static bool IsNaN(this double value)
        {
            return double.IsNaN(value);
        }


        public static bool[] ToBoolArray(this IList<string> List, bool? ValueIfFail = null)
        {
            bool[] Result = new bool[List.Count];
            for (int n = 0; n < List.Count; n++)
            {
                var Item = List[n];
                Result[n] = Item.ToBool(ValueIfFail ?? false);
            }
            return Result;
        }


        public static Task<T> ToTask<T>(this T value)
        {
            return Task.FromResult<T>(value);
        }

        public static bool? ToBoolNull(this object value, bool? ValueIfError = null)
        {
            try
            {
                //if ((string)value == "1") return true;
                //if ((string)value == "0") return false;
                if (value is string S)
                {
                    string SL = S.ToLower();
                    if (S == "0" || SL == "false" || SL == "close" || SL == "no" || SL == "off") return false;
                    if (S == "1" || SL == "true" || SL == "open" || SL == "yes" || SL == "on") return true;
                    return null;
                }
                return Convert.ToBoolean(value);
            }
            catch { return ValueIfError; }
        }




        public static double[] ToDouble(this IList List, double ValueIfFail = double.NaN)
        {
            double[] Result = new double[List.Count];
            for (int n = 0; n < List.Count; n++)
            {
                var Item = List[n];
                Result[n] = Item.ToDouble(ValueIfFail);
            }
            return Result;
        }


        public static double ToDouble(this object value, double ValueIfFail = double.NaN)
        {
            if (value == null) return ValueIfFail;
            try
            {
                return Convert.ToDouble(value);
            }
            catch
            {
                return ValueIfFail;
            }
        }


        public static decimal ToDecimal(this object value, decimal ValueIfFail = decimal.Zero)
        {
            if (value == null) return ValueIfFail;
            decimal Temp = decimal.Zero;
            try
            {
                Temp = Convert.ToDecimal(value);
            }
            catch
            {
            }
            return Temp;
        }

        [DebuggerStepThrough]
        public static float ToFloat(this object value)
        {
            if (value == null) return float.NaN;
            float Temp = float.NaN;
            try
            {
                Temp = Convert.ToSingle(value);
            }
            catch
            {
            }
            return Temp;
        }




        public static double? ToDoubleNull(this object value)
        {
            if (value == null) return null;
            //if (value.ToString() == "") return null;
            double? Temp;
            try
            {
                Temp = Convert.ToDouble(value);
            }
            catch
            {
                return null;
            }
            if (Temp == double.NaN) return null;
            return Temp;
        }

        public static string ToIfWhiteSpaceToNull(this string value)
        {
            if (value.IsNullOrWhiteSpace()) return null; else return value;
        }

        public static string ToStringFromBase64(this string encodedString)
        {
            byte[] data = Convert.FromBase64String(encodedString);
            string decodedString = Encoding.UTF8.GetString(data);
            return decodedString;
        }

        public static string ToBase64(this string text)
        {
            return ToBase64(text, Encoding.UTF8);
        }

        public static string ToBase64(this string text, Encoding encoding)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            byte[] textAsBytes = encoding.GetBytes(text);
            return Convert.ToBase64String(textAsBytes);
        }

        public static bool TryParseBase64(this string text, out string decodedText)
        {
            return TryParseBase64(text, Encoding.UTF8, out decodedText);
        }

        public static bool TryParseBase64(this string text, Encoding encoding, out string decodedText)
        {
            if (string.IsNullOrEmpty(text))
            {
                decodedText = text;
                return false;
            }

            try
            {
                byte[] textAsBytes = Convert.FromBase64String(text);
                decodedText = encoding.GetString(textAsBytes);
                return true;
            }
            catch (Exception)
            {
                decodedText = null;
                return false;
            }
        }

        public static string ToStringFromIComparable(this IComparable value)
        {
            if (value != null) return value.ToString(); else return null;
        }


        public static double ToDouble(this object value, out string ErrorLog)
        {
            double Temp = double.NaN;
            ErrorLog = "";
            try
            {
                Temp = Convert.ToDouble(value);
            }
            catch (Exception ex)
            {
                ErrorLog = "ERROR: Unable to Convert object '" + value.ToString() + "' to Double DataType.\r\n" + ex.ToString();
            }
            return Temp;
        }

        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long ToUnixTimeInMS_FromLocalTime(this DateTime dateTime)
        {
            return (long)(dateTime.ToUniversalTime() - UnixEpoch).TotalMilliseconds;
        }

        public static long ToUnixTimeInMS_FromUTCTime(this DateTime dateTime)
        {
            return (long)(dateTime - UnixEpoch).TotalMilliseconds;
        }

        public static long ToUnixTimeInS_FromLocalTime(this DateTime dateTime)
        {
            return (long)(dateTime.ToUniversalTime() - UnixEpoch).TotalSeconds;
        }

        public static long ToUnixTimeInS_FromUTCTime(this DateTime dateTime)
        {
            return (long)(dateTime - UnixEpoch).TotalSeconds;
        }

        public static Stream ToStream(this string strIn)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(strIn));


            //var stream = new MemoryStream();
            //var writer = new StreamWriter(stream);
            //writer.Write(strIn);
            //writer.Flush();
            //stream.Position = 0;
            //return stream;
        }

        public static string ToFirstRegexWithStartFrom(this string StrIn, string PatternStartFrom, string Pattern, out string StartFromRegexMatch, RegexOptions regexOptions = RegexOptions.IgnoreCase)
        {
            string StrOut = null;
            StartFromRegexMatch = null;
            try
            {
                //StartFromRegexMatch

                int SearchFrom = 0;
                if (!PatternStartFrom.IsNullOrEmpty())
                {
                    MatchCollection FirstMC = Regex.Matches(StrIn, PatternStartFrom, regexOptions);
                    if (FirstMC.Count > 0)
                    {
                        StartFromRegexMatch = FirstMC[0].Value;
                        SearchFrom = FirstMC[0].Index + FirstMC[0].Length;
                    }
                }


                if (!Pattern.IsNullOrEmpty())
                {
                    string ToSearch = StrIn.Substring(SearchFrom);
                    MatchCollection MC = Regex.Matches(ToSearch, Pattern, regexOptions);
                    if (MC.Count > 0) StrOut = MC[0].Value;
                }
            }
            catch
            {
            }

            return StrOut;
        }

        public static string ToFirstRegexWithStartFrom(this string StrIn, string PatternStartFrom, string Pattern, RegexOptions regexOptions = RegexOptions.IgnoreCase)
        {
            try
            {
                if (Pattern.IsNullOrEmpty()) return "";
                int SearchFrom = 0;
                if (!PatternStartFrom.IsNullOrEmpty())
                {
                    MatchCollection FirstMC = Regex.Matches(StrIn, PatternStartFrom, regexOptions);
                    if (FirstMC.Count > 0) SearchFrom = FirstMC[0].Index + FirstMC[0].Length;
                }
                string ToSearch = StrIn.Substring(SearchFrom);
                MatchCollection MC = Regex.Matches(ToSearch, Pattern, regexOptions);
                if (MC.Count > 0) return MC[0].Value; else return "";
            }
            catch
            {
                return "";
            }
        }

        public static bool IsValidIPAddress(this string IPStr)
        {
            try
            {
                return (IPAddress.TryParse(IPStr, out IPAddress address));
            }
            catch
            {
                return false;
            }
        }


        public static DateTime ToDateTimeUTC_ParseTimeStampSemantic(this string TSStr, bool DateTimeInputIsUTC = true, DateTime? LastDateTimeNow = null)
        {
            string TSStrNoSpaces = TSStr?.Replace(" ", string.Empty);
            DateTime DT = DateTime.MinValue;
            if (TSStrNoSpaces.Length >= 3 && TSStrNoSpaces.Substring(0, 3).ToLower() == "now")
            {
                DT = LastDateTimeNow ?? DateTime.UtcNow;
                if (TSStrNoSpaces.Length >= 5)
                {
                    bool Add = false, Subtract = false;
                    if (TSStrNoSpaces[3] == '+') Add = true;
                    if (TSStrNoSpaces[3] == '-') Subtract = true;
                    string OffsetStr = TSStrNoSpaces.Substring(4, TSStrNoSpaces.Length - 4).FindFirstRegexMatch("^([0-9]*\\.[0-9]+|[0-9]+)");
                    if (OffsetStr != "")
                    {
                        double Offset = 0;
                        if (Add) Offset = double.Parse(OffsetStr);
                        else if (Subtract) Offset = double.Parse(OffsetStr) * -1;
                        else return DateTime.MinValue;
                        string UnitTS = TSStrNoSpaces.Substring(4 + OffsetStr.Length, TSStrNoSpaces.Length - 4 - OffsetStr.Length);
                        switch (UnitTS.ToLower())
                        {
                            case "s":
                            case "sec":
                            case "secs":
                            case "second":
                            case "seconds":
                                DT = DT.AddSeconds(Offset); break;
                            case "m":
                            case "min":
                            case "mins":
                            case "minute":
                            case "minutes":
                                DT = DT.AddMinutes(Offset); break;
                            case "h":
                            case "hr":
                            case "hrs":
                            case "hour":
                            case "hours":
                                DT = DT.AddHours(Offset); break;
                            case "d":
                            case "dy":
                            case "dys":
                            case "day":
                            case "days":
                                DT = DT.AddDays(Offset); break;
                            default:
                                DT = DateTime.MinValue; break;
                        }
                    }
                }
            }
            else DT = TSStr.ToDateTime(DateTimeInputIsUTC);
            return DT;
        }

        public static string ToLegalFileName(this string FilePathName)
        {
            string FN = FilePathName;
            if (FN == "") return FN;
            string[] InvalidNames = new string[] { "CON", "PRN", "AUX", "CLOCK$", "NUL", "COM0", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT0", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9" };
            foreach (string s in InvalidNames)
            {
                if (FN == s) FN = "-{[" + s + "]}-";
                FN = FN?.Replace(s + ".", "-{[" + s + "]}-.");
            }
            foreach (char c in System.IO.Path.GetInvalidFileNameChars()) //This contains the System.IO.Path.GetInvalidPathChars() Characters as well 
            {
                FN = FN?.Replace(c.ToString(), "-{[" + (int)c + "]}-");
            }
            if (FN.Substring(0, 1) == ".")
            {
                FN = "-{[46]}-" + FN.Substring(1, FN.Length - 1);
            }
            if (FN.Right(1) == ".")
            {
                FN = FN.Substring(0, FN.Length - 1) + "-{[46]}-";
            }
            return FN;
        }


        public static string ToIllegalFileName_FromLegalName(this string IllegalFN)
        {
            string FN = IllegalFN;
            string[] InvalidNames = new string[] { "CON", "PRN", "AUX", "CLOCK$", "NUL", "COM0", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT0", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9" };
            foreach (string s in InvalidNames)
            {
                FN = FN.Replace("-{[" + s + "]}-", s);
            }
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            {
                FN = FN.Replace("-{[" + (int)c + "]}-", c.ToString());
            }
            FN = FN.Replace("-{[46]}-", ".");
            return FN;
        }


        public static int WordCount(this string str)
        {
            string[] userString = str.Split(new char[] { ' ', '.', '?' },
                                        StringSplitOptions.RemoveEmptyEntries);
            int wordCount = userString.Length;
            return wordCount;
        }

        public static int LastDayOfTheMonth(this DateTime DT)
        {
            return DateTime.DaysInMonth(DT.Year, DT.Month);
        }

        public static DateTime FirstDayInMonth(this DateTime DT, DayOfWeek D)
        {
            DateTime Temp = new DateTime(DT.Year, DT.Month, 1, DT.Hour, DT.Minute, DT.Second);
            while (Temp.DayOfWeek != D) Temp = Temp.AddDays(1);
            return Temp;
        }

        public static DateTime LastDayInMonth(this DateTime DT, DayOfWeek D)
        {
            DateTime Temp = new DateTime(DT.Year, DT.Month, DateTime.DaysInMonth(DT.Year, DT.Month), DT.Hour, DT.Minute, DT.Second);
            while (Temp.DayOfWeek != D) Temp = Temp.AddDays(-1);
            return Temp;
        }

        public static int TotalCharWithoutSpace(this string str)
        {
            int totalCharWithoutSpace = 0;
            string[] userString = str.Split(' ');
            foreach (string stringValue in userString)
            {
                totalCharWithoutSpace += stringValue.Length;
            }
            return totalCharWithoutSpace;
        }

        public static int GetWeekOfMonth(this DateTime time, CalendarWeekRule CWR, DayOfWeek DOW)
        {
            DateTime first = new DateTime(time.Year, time.Month, 1);
            return time.GetWeekOfYear(CWR, DOW) - first.GetWeekOfYear(CWR, DOW) + 1;
        }

        public static DateTime LastDateTimeOfWeek(this DateTime time)
        {
            int Offset = 7 - (int)time.DayOfWeek;
            return time.AddDays(Offset);
        }


        public static DateTime FirstDateInWeek(this DateTime dt)
        {
            while (dt.DayOfWeek != System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.FirstDayOfWeek)
                dt = dt.AddDays(-1);
            return dt;
        }

        public static DateTime FirstDateInWeek(this DateTime dt, DayOfWeek weekStartDay)
        {
            while (dt.DayOfWeek != weekStartDay)
                dt = dt.AddDays(-1);
            return dt;
        }

        public static IEnumerable<T> DequeueChunk<T>(this Queue<T> queue, int chunkSize)
        {
            for (int i = 0; i < chunkSize && queue.Count > 0; i++)
            {
                yield return queue.Dequeue();
            }
        }


        public static T Last_NonEnumerating<K, T>(this SortedList<K, T> Collection)
        {
            T RetVal = Collection.Values[Collection.Count - 1];
            return RetVal;
        }

        public static void RemoveFirst<T>(this IList<T> Collection, Func<T, bool> Condition)
        {
            T ItemToRemove = default(T);
            bool ItemFound = false;
            foreach (T Item in Collection)
            {
                if (Condition(Item)) { ItemToRemove = Item; ItemFound = true; break; }
            }
            if (ItemFound) Collection.Remove(ItemToRemove);
        }

        public static IEnumerable<T> TakeLast<T>(this IEnumerable<T> source, int N)
        {
            return source.Skip(Math.Max(0, source.Count() - N));
        }


        public static void RemoveWhere<TKey, TValue>(this IDictionary<TKey, TValue> Collection, Func<TKey, TValue, bool> Condition)
        {
            List<TKey> ItemsToRemove = new List<TKey>();
            foreach (KeyValuePair<TKey, TValue> KVP in Collection) if (Condition(KVP.Key, KVP.Value)) { ItemsToRemove.Add(KVP.Key); }
            foreach (TKey ItemToRemove in ItemsToRemove) Collection.Remove(ItemToRemove);
        }

        public static void RemoveWhere<T>(this IList<T> Collection, Func<T, bool> Condition)
        {
            List<T> ItemsToRemove = new List<T>();
            foreach (T Item in Collection) if (Condition(Item)) { ItemsToRemove.Add(Item); }
            foreach (T ItemToRemove in ItemsToRemove) Collection.Remove(ItemToRemove);
        }

        public static T FirstOrDefaultWithIndex<T>(this IEnumerable<T> Collection, Func<T, bool> Condition, out int Index)
        {
            int n = 0;
            foreach (T Item in Collection)
            {
                if (Condition(Item))
                {
                    Index = n;
                    return Item;
                }
                n++;
            }
            Index = -1;
            return default(T);
        }

        public static void RemoveWhere(this IList Collection, Func<object, bool> Condition)
        {
            List<object> ItemsToRemove = new List<object>();
            for (int n = 0; n < Collection.Count; n++)
            {
                object Item = Collection[n];
                if (Condition(Item)) { ItemsToRemove.Add(Item); }
            }
            foreach (object ItemToRemove in ItemsToRemove) Collection.Remove(ItemToRemove);
        }

        public static string ToEnumDescription(this Enum value)
        {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            if (name != null)
            {
                FieldInfo field = type.GetField(name);
                if (field != null)
                {
                    DescriptionAttribute attr =
                           System.Attribute.GetCustomAttribute(field,
                             typeof(DescriptionAttribute)) as DescriptionAttribute;
                    if (attr != null)
                    {
                        return attr.Description;
                    }
                }
            }
            return null;
        }

        public static string ToSimplifiedOPScript(this string FullCode, out string TopPart, out string BottomPart)
        {
            string StrStart = @"\/\/\*\*\*\*\*\*\*\*\*\*\* ANALYTIC SCRIPT STARTS HERE \(DO NOT REMOVE THIS LINE\) \*\*\*\*\*\*\*\*\*\*\*[\r\n]?";
            //string StrStart = @"//*********** ANALYTIC SCRIPT STARTS HERE (DO NOT REMOVE THIS LINE) ***********";
            string StrEnd = @"[\r\n]?\/\/\*\*\*\*\*\*\*\*\*\*\* ANALYTIC SCRIPT ENDS HERE \(DO NOT REMOVE THIS LINE\) \*\*\*\*\*\*\*\*\*\*\*\*\*";
            if (Regex.IsMatch(FullCode, StrStart) && Regex.IsMatch(FullCode, StrEnd))
            {
                //Start = ScriptCode.IndexOf(StrStart, 0) + StrStart.Length;
                Match M = Regex.Match(FullCode, StrStart);
                int Start = M.Index + M.Length;
                int End = Regex.Match(FullCode, StrEnd).Index;
                TopPart = FullCode.Substring(0, Start);
                BottomPart = FullCode.Substring(End, FullCode.Length - End);
                string RegexResult = FullCode.Substring(Start, End - Start);
                return RegexResult;
            }
            else
            {
                TopPart = null;
                BottomPart = null;
                return FullCode;
                //throw new Exception("Unable to obtain simplified script code from full code =>\r\n" + FullCode);
            }
        }


        public static bool Contains(this string text, string value, StringComparison stringComparison)
        {
            return text.IndexOf(value, stringComparison) >= 0;
        }



        public static string ReplacePasswordFieldInAPICall(this string Input)
        {
            var k = Regex.Replace(Input, "(?<=password=)[^&]*", "********", RegexOptions.IgnoreCase);
            return k;
        }



        public static bool TryParseOPURLPath(this string URLPathOnly, out string[] Folders)
        {
            Folders = URLPathOnly.Split('/');
            if (Folders.Length <= 1) //This means no '/' was specified..
            {
                return false;
            }
            if (Folders.Length <= 2) //Only one '/' was specified
            {
                switch (Folders[0])
                {
                    case "Tags":
                        break;

                }
            }


            return true;
        }


        public static bool IsValidGuid(this string GuidStr)
        {
            try
            {
                Guid guidResult = Guid.Parse(GuidStr);
            }
            catch
            {
                return false;
            }
            return true;
        }


        public static bool IsJSONString(this string StrIn)
        {
            string StrInTrimmed = StrIn.Trim();
            return (StrInTrimmed.StartsWith("{") && StrInTrimmed.EndsWith("}") || (StrInTrimmed.StartsWith("[") && StrInTrimmed.EndsWith("]")));
        }

        //public static OPDataType ToOPParamDataType(this object ParameterValue)
        //{
        //    try
        //    {
        //        Type ParameterValueType = ParameterValue.GetType();
        //        if (ParameterValue.IsIntType())
        //        {
        //            return OPDataType.Int;
        //        }
        //        else if (ParameterValue.IsRealType())
        //        {
        //            return OPDataType.Real;
        //        }
        //        else if (ParameterValueType == typeof(DateTime))
        //        {
        //            return OPDataType.DateTime;
        //        }
        //        else if (ParameterValueType == typeof(bool))
        //        {
        //            return OPDataType.Bool;
        //        }
        //        else
        //        {
        //            return OPDataType.String;
        //        }
        //    }
        //    catch
        //    {
        //        return OPDataType.String;
        //    }
        //}
        public static string ToStringCombined(this IEnumerable<string> source, string CombiningChar = ",")
        {
            string Res = null;
            foreach (string el in source)
            {
                Res = Res + el + CombiningChar;
            }
            if (Res.Length > 0) Res.RemoveLastCharacter();
            return Res;
        }


        /// <summary>
        /// Changes a string which has "\r" into actual carriage return
        /// </summary>
        public static string ToUnescapeString(this string s) { return Regex.Unescape(s); }

        /// <summary>
        /// Changes a string which actual carriage return into "\r" (or "\\r" in c#)
        /// </summary>
        public static string ToEscapeString(this string s) { return Regex.Escape(s); }



        public static List<string> RemoveNullOrEmptyStrings(this List<string> Input)
        {
            for (int n = Input.Count() - 1; n >= 0; n--) if (Input[n].IsNullOrEmpty()) Input.RemoveAt(n);
            return Input;
        }


        public static bool IsAsyncAction(this Action action)
        {
            return action.Method.IsDefined(typeof(AsyncStateMachineAttribute), false);
        }

        public static bool IsAsyncAction(this Action<object> action)
        {
            return action.Method.IsDefined(typeof(AsyncStateMachineAttribute), false);
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


        public static bool EndsWith(this string value, char charToCompare)
        {
            if (value.Length == 0) return false;
            else return value[value.Length - 1] == charToCompare;
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



        public static string ReadToEnd(this MemoryStream BASE)
        {
            BASE.Position = 0;
            StreamReader R = new StreamReader(BASE);
            return R.ReadToEnd();
        }




        [DebuggerStepThrough]
        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
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


        public static string FindFirstRegexMatch(this string Input, string Pattern)
        {
            Match Match = Regex.Match(Input, Pattern, RegexOptions.IgnoreCase);
            if (Match.Success)
            {
                return Match.Value;
            }
            return "";
        }


        static GregorianCalendar _gc = new GregorianCalendar();

        public static int GetWeekOfYear(this DateTime time, CalendarWeekRule CWR, DayOfWeek DOW)
        {
            return _gc.GetWeekOfYear(time, CWR, DOW);
        }


    }
}
