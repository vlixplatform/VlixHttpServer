using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Threading;
using MaterialIcons;
using System.Reflection;

namespace Vlix.ServerConfigUI
{

    public class OPTextBox : TextBox
    {
        
        public OPTextBox()
        {
        }


        public string PlaceHolder
        {
            get { return (string)GetValue(PlaceHolderProperty); }
            set { SetValue(PlaceHolderProperty, value); }
        }
        public static readonly DependencyProperty PlaceHolderProperty = DependencyProperty.Register("PlaceHolder", typeof(string), typeof(OPTextBox), new PropertyMetadata(null));




    }


}
