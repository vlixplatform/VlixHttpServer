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

namespace Vlix.ServerConfigUI
{
    /// <summary>
    /// Interaction logic for UCServerConfig.xaml
    /// </summary>
    public partial class UCServerConfig : UserControl
    {
        public UCServerConfig()
        {
            InitializeComponent();
        }
        private void opbRefresh_Click(object sender, RoutedEventArgs e)
        {
            //uCHttpServerConfig.OnRefreshClick
        }
        private async Task UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await uCHttpServerConfig.Initialize(
                (vM) => {
                    
                }, 
                null
            );
        }
        private void miExit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }
        private void opbRemote_Click(object sender, RoutedEventArgs e)
        {

        }
        private void opfRemoteSettingsWindow_Close(object sender, RoutedEventArgs e)
        {
            ((ServerConfigVM)this.DataContext).ShowRemoteSettingsWindow = false;
        }
        private void opbRemoteAccept_Click(object sender, RoutedEventArgs e)
        {
            var configVM = ((ServerConfigVM)this.DataContext);
            configVM.ConfigRemoteUsername = tbConfigRemoteUsername.Text;
            configVM.ConfigRemotePassword = pbConfigRemotePassword.Password;
            configVM.ConfigRemoteEnable = cbConfigRemoteEnable.IsChecked ?? false;
        }

        public Action<HttpServerConfigVM> OnSaveAndApply;
        private void opbSaveApply_Click(object sender, RoutedEventArgs e)
        {
            this.OnSaveAndApply?.Invoke(((HttpServerConfigVM)this.DataContext));
        }
    }
}
