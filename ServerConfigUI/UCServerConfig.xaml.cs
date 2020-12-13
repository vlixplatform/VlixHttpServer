using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
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
    public partial class UCServerConfig : UserControl
    {
        ServerConfigVM configVM;
        HttpServerConfigVM httpServerConfigVM;
        public UCServerConfig()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Settings settings = Services.LoadConfigFile<Settings>("settings.json");
            configVM = ((ServerConfigVM)this.DataContext);
            configVM.Host = settings.LastHost;
            configVM.Username = settings.LastUsername;
            Services.Config = settings;

            uCHttpServerConfig.OnRefresh = async () =>
            {
                HttpServerConfig newServerConfig = await this.OnRefresh?.Invoke(configVM.Host, configVM.Username, configVM.Password);
                return newServerConfig;
            };

            uCHttpServerConfig.OnSaveAndApply = async (httpServerConfig) =>
            {
                configVM.HttpServerConfigVM.Update(httpServerConfig);
                return await this.OnSaveAndApply?.Invoke(configVM);
            };
            httpServerConfigVM = (HttpServerConfigVM)uCHttpServerConfig.DataContext;

            httpServerConfigVM.OnSSLCertRefresh = async (storeName, storeLocation) =>
            {
                using (OPHttpClient httpClient = new OPHttpClient())
                {
                    List<SSLCertVM> Res = await httpClient.RequestJsonAsync<List<SSLCertVM>>(HttpMethod.Get, configVM.Host + "/config/getsslcerts?storename=" + storeName + "&storelocation=" + storeLocation, configVM.Username, configVM.Password);
                    return Res;
                }
            };
            
        }
        //public void Initialize(Func<HttpServerConfigVM, Task> onSaveAndApply, Func<string, string, string, Task<HttpServerConfigVM>> onRefresh)
        //{
        //    this.OnSaveAndApply = onSaveAndApply;
        //    this.OnRefresh = onRefresh;
        //}
        private async void miRefresh_Click(object sender, RoutedEventArgs e)
        {
            WebServerConfig newWebServerConfig = await this.OnRefresh?.Invoke(configVM.Host, configVM.ConfigUsername, configVM.Password);
            if (newWebServerConfig != null)
            {
                configVM.ConfigSSLCertificateStoreName = newWebServerConfig.ConfigUtility.SSLCertificateStoreName;
                configVM.ConfigSSLCertificateSubjectName = newWebServerConfig.ConfigUtility.SSLCertificateSubjectName;
                configVM.ConfigHTTPPort = newWebServerConfig.ConfigUtility.HTTPPort;
                configVM.ConfigEnableHTTPS = newWebServerConfig.ConfigUtility.EnableHTTPS;
                configVM.ConfigLocalhostOnly = newWebServerConfig.ConfigUtility.AllowLocalhostConnectionsOnlyForHttp;
                configVM.HttpServerConfigVM.Update(newWebServerConfig);                
            }
        }
    
        private void miExit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }
        private void opfPasswordSettingsWindow_Close(object sender, RoutedEventArgs e)
        {
            ((ServerConfigVM)this.DataContext).ShowPasswordSettingsWindow = false;
        }
        private void opbRemoteAccept_Click(object sender, RoutedEventArgs e)
        {
            cmConfig.ShowMessageProcess("Saving...");
            if (pbConfigPassword.Password == pbConfigPasswordRetype.Password)
            {
                configVM.ConfigPasswordHash = pbConfigPassword.Password.ToSha256();
            }
            else cmConfig.ShowMessageError("'Password Retype' does not match 'Password'");
        }

        public Func<ServerConfigVM,Task<bool>> OnSaveAndApply;
        public Func<string, string, string, Task<WebServerConfig>> OnRefresh;
        private async void opbLogin_Click(object sender, RoutedEventArgs e)
        {
            cmLogin.ShowMessageProcess("Logging in...");
            configVM.Host = optbHost.Text;
            configVM.IsLoggingIn = true;
            WebServerConfig newHttpServerConfig = await this.OnRefresh?.Invoke(configVM.Host, configVM.Username, pbConfigPassword.Password);
            if (newHttpServerConfig != null)
            {
                ((HttpServerConfigVM)uCHttpServerConfig.DataContext).Update(newHttpServerConfig);

                //configVM.HttpServerConfigVM.Update(newHttpServerConfig);
                //configVM.HttpServerConfigVM = new HttpServerConfigVM(newHttpServerConfig);
                //uCHttpServerConfig.DataContext = null;
                //uCHttpServerConfig.DataContext = configVM.HttpServerConfigVM;

                Settings settings = ((Settings)Services.Config);
                settings.LastHost = configVM.Host;
                settings.LastUsername = configVM.Username;
                Services.SaveConfigFile("settings.json", settings);

                cmLogin.ShowMessageSuccess("Login Success!");
                configVM.ShowLoginWindow = false;
            }
            else
            {
                cmLogin.ShowMessageError("Login Failed!");
            }
            configVM.IsLoggingIn = false;
        }


        private void loginKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                opbLogin_Click(null, null);
            }
        }

        private void optbHost_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            configVM.Host = optbHost.PlaceHolder;
        }

        private void opfConfigSelectSSLCert_OnClose(object sender, RoutedEventArgs e)
        {
            configVM.ShowConfigSelectSSLCertWindow = false;
        }

        private void UCConfigSelectSSLCert_OnCertificateSelected(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is SSLCertVM sSLCertVM)
            {
                configVM.ConfigSSLCertificateSubjectName = sSLCertVM.Subject;
                configVM.ConfigSSLCertificateStoreName = sSLCertVM.StoreName;
                configVM.ConfigSubjectAlternativeNames.Clear();
                foreach (var s in sSLCertVM.SubjectAlternativeNames) configVM.ConfigSubjectAlternativeNames.Add(s);
                configVM.ShowConfigSelectSSLCertWindow = false;
            }
        }

    }
}
