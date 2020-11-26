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
    public partial class UCSelectSSLCert : UserControl
    {
        SelectSSLCertVM selectSSLCertVM = null;

        public UCSelectSSLCert()
        {
            InitializeComponent();
            
            
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            selectSSLCertVM = (SelectSSLCertVM)this.DataContext;
            await selectSSLCertVM.LoadVM();
        }

        private async void sslStore_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectSSLCertVM = (SelectSSLCertVM)this.DataContext;
            await selectSSLCertVM.LoadVM();
        }
    }
}
