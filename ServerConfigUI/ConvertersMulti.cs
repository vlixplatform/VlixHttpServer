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
using System.Windows.Media;
using System.Windows;
using System.ComponentModel;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Shapes;

using System.Windows.Markup;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;


namespace Vlix.ServerConfigUI
{
    public class Combine2StringWithSpaceBetweenMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values[0] + " " + values[1];
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            string[] splitValues = ((string)value).Split(' ');
            return splitValues;
        }
    }

    public class Combine2StringNoSpaceBetweenMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return (string)values[0] + (string)values[1];
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class BoolANDMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is bool v1 && values[1] is bool v2) return v1 && v2; else return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class IntV1LargerOrEqualIntV2ToBoolMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is int v1 && values[1] is int v2) return v1 >= v2; else return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolORToFalseMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            for (int nV1 = 0; nV1 < values.Count(); nV1++)
            {
                for (int nV2 = nV1; nV2 < values.Count(); nV2++)
                {
                    if (values[nV1] is bool v1 && values[nV2] is bool v2)
                    {
                        if (v1 || v2) return false;
                    }
                }
            }
            return true;
            // if (values[0] is bool v1 && values[1] is bool v2) return v1 || v2; else return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolORMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            for (int nV1 = 0; nV1 < values.Count(); nV1++)
            {
                for (int nV2 = nV1; nV2 < values.Count(); nV2++)
                {
                    if (values[nV1] is bool v1 && values[nV2] is bool v2)
                    {
                        if (v1 || v2) return true;
                    }
                }
            }
            return false;
            // if (values[0] is bool v1 && values[1] is bool v2) return v1 || v2; else return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolTrueToOpacity1Else05_ORMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is bool v1 && values[1] is bool v2)
            {
                bool Res = v1 || v2;
                if (Res) return (double)1; else return (double)0.5;
            }
            return (double)0.5;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolXORMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is bool v1 && values[1] is bool v2) return v1 ^ v2; else return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }





    public class BooleanAndConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            foreach (object value in values)
            {
                if ((value is bool) && (bool)value == false) return false;
            }
            return true;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("BooleanAndConverter is a OneWay converter.");
        }
    }

}
