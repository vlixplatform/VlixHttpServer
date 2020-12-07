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

namespace Vlix.HttpServerConfig
{
    /// <summary>
    /// Interaction logic for UCHttpServerConfig.xaml
    /// </summary>
    public partial class UCHttpServerConfig : UserControl
    {
        public UCHttpServerConfig()
        {
            InitializeComponent();
            var ddd = ((HttpServerConfigVM)this.DataContext);
        }
        private void opfSelectSSLCert_OnClose(object sender, RoutedEventArgs e)
        {
            ((HttpServerConfigVM)this.DataContext).ShowSelectSSLCertWindow = false;
        }
        private void opbNewRule_Click(object sender, RoutedEventArgs e)
        {
            svRules.ScrollToBottom();
        }

        private void UCSelectSSLCert_OnCertificateSelected(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is SSLCertVM sSLCertVM)
            {
                HttpServerConfigVM httpServerConfigVM = ((HttpServerConfigVM)this.DataContext);
                httpServerConfigVM.SSLCertificateSubjectName = sSLCertVM.Subject;
                httpServerConfigVM.SSLCertificateStoreName = sSLCertVM.StoreName;
                httpServerConfigVM.SubjectAlternativeNames.Clear();
                foreach (var s in sSLCertVM.SubjectAlternativeNames) httpServerConfigVM.SubjectAlternativeNames.Add(s);
                httpServerConfigVM.ShowSelectSSLCertWindow = false;
            }
        }
    }
}
