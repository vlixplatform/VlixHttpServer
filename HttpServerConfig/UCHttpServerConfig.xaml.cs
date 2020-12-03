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
using Vlix.HttpServer;

namespace HttpServerConfig
{
    /// <summary>
    /// Interaction logic for UCHttpServerConfig.xaml
    /// </summary>
    public partial class UCHttpServerConfig : UserControl
    {
        public UCHttpServerConfig()
        {
            InitializeComponent();
        }
        private void opfSelectSSLCert_OnClose(object sender, RoutedEventArgs e)
        {
            ((HttpServerConfigVM)this.DataContext).ShowSelectSSLCertWindow = false;
        }
        private void opbNewRule_Click(object sender, RoutedEventArgs e)
        {
            svRules.ScrollToBottom();
        }
    }
}
