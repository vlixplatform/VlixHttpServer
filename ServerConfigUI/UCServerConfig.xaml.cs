using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
            httpServerConfigVM = (HttpServerConfigVM)uCHttpServerConfig.DataContext;
            Settings settings = Services.LoadConfigFile<Settings>("settings.json");
            configVM = ((ServerConfigVM)this.DataContext);
            configVM.Host = settings.LastHost;
            configVM.Username = settings.LastUsername;
            Services.Config = settings;

            httpServerConfigVM.OnRefresh = async () =>
            {
                try
                {
                    Tuple<WebServerConfig, SSLCertVM, SSLCertVM> obj = await Services.OPHttpClient.RequestJsonAsync<Tuple<WebServerConfig, SSLCertVM, SSLCertVM>>(HttpMethod.Get, configVM.Host + "/config/load", configVM.Username, configVM.Password);
                    return new Tuple<HttpServerConfig, SSLCertVM>(obj.Item1, obj.Item2);
                }
                catch (Exception ex)
                {
                    return null;
                }
            };

            httpServerConfigVM.OnLogRefresh = async (lastLogReadUTC) =>
            {
                try
                {
                    List<LogStruct> logs = await Services.OPHttpClient.RequestJsonAsync<List<LogStruct>>(HttpMethod.Get, configVM.Host + "/config/getlogs?lastread="+ lastLogReadUTC.Ticks, configVM.Username, configVM.Password);
                    return logs;
                }
                catch (Exception ex)
                {
                    return null;
                }
            };
            httpServerConfigVM.CheckConnectionOK = () => { return  Task.FromResult(configVM.LoggedIn); };

            httpServerConfigVM.OnSaveAndApply = async (httpServerConfig) =>
            {
                try
                {
                    UtilityConfig utilityConfig = new UtilityConfig()
                    {
                        AllowLocalhostConnectionsOnlyForHttp = configVM.ConfigLocalhostOnly,
                        SSLCertificateStoreName = configVM.ConfigSSLCertificateStoreName,
                        SSLCertificateSubjectName = configVM.ConfigSSLCertificateSubjectName,
                        EnableHTTPS = configVM.ConfigEnableHTTPS,
                        ConfigUsername = configVM.ConfigUsername,
                        HTTPPort = configVM.ConfigHTTPPort,
                        HTTPSPort = configVM.ConfigHTTPSPort,
                        ConfigPasswordHash = configVM.ConfigPasswordHash
                    };
                    WebServerConfig webServerConfig = new WebServerConfig(httpServerConfig, utilityConfig);
                    var res = await Services.OPHttpClient.RequestStatusOnlyAsync(HttpMethod.Put, configVM.Host + "/config/save", configVM.Username, configVM.Password, webServerConfig);
                    if (res == HttpStatusCode.OK) return true;
                }
                catch
                {
                    return false;
                }
                return false;
            };

            httpServerConfigVM.OnSSLCertRefresh = async (storeName, storeLocation) =>
            {
                using (OPHttpClient httpClient = new OPHttpClient())
                {
                    List<SSLCertVM> Res = await httpClient.RequestJsonAsync<List<SSLCertVM>>(HttpMethod.Get, configVM.Host + "/config/getsslcerts?storename=" + storeName + "&storelocation=" + storeLocation, configVM.Username, configVM.Password);
                    return Res;
                }
            };
            ((SelectSSLCertVM)uCConfigSelectSSLCert.DataContext).OnRefresh = (storeName, storeLocation) =>
            {
                return httpServerConfigVM.OnSSLCertRefresh?.Invoke(storeName, storeLocation);
            };
        }
        //public void Initialize(Func<HttpServerConfigVM, Task> onSaveAndApply, Func<string, string, string, Task<HttpServerConfigVM>> onRefresh)
        //{
        //    this.OnSaveAndApply = onSaveAndApply;
        //    this.OnRefresh = onRefresh;
        //}
        private async void miRefresh_Click(object sender, RoutedEventArgs e)
        {
            configVM.IsLoading = true;
            cmServerConfig.ShowMessageProcess("Refreshing...");
            try
            {
                Tuple<WebServerConfig, SSLCertVM, SSLCertVM> obj = await Services.OPHttpClient.RequestJsonAsync<Tuple<WebServerConfig, SSLCertVM, SSLCertVM>>(HttpMethod.Get, configVM.Host + "/config/load", configVM.ConfigUsername, configVM.Password);
                WebServerConfig newWebServerConfig = obj.Item1;
                if (newWebServerConfig != null)
                {
                    configVM.Update(newWebServerConfig, obj.Item3.SubjectAlternativeNames);
                    pbConfigPassword.Password = Services.PasswordField;
                    pbConfigPasswordRetype.Password = Services.PasswordField;
                    ((HttpServerConfigVM)uCHttpServerConfig.DataContext).Update(newWebServerConfig, obj.Item2.SubjectAlternativeNames);
                    await Task.Delay(500);
                    cmServerConfig.StopMessage();
                }
            }
            catch
            {
                cmServerConfig.ShowMessageError("Refresh Failed!");
            }
            configVM.IsLoading = false;
        }
    
        private void opfSettingsWindow_OnShow(object sender, RoutedEventArgs e)
        {
            configVM.NewConfigEnableHTTPS = configVM.ConfigEnableHTTPS;
            configVM.NewConfigHTTPPort = configVM.ConfigHTTPPort;
            configVM.NewConfigLocalhostOnly = configVM.ConfigLocalhostOnly;
            configVM.NewConfigUsername = configVM.ConfigUsername;
            configVM.NewConfigSSLCertificateSubjectName = configVM.ConfigSSLCertificateSubjectName;
            configVM.NewConfigSSLCertificateStoreName = configVM.ConfigSSLCertificateStoreName;
            configVM.NewConfigHTTPSPort = configVM.ConfigHTTPSPort;
        }
        private void opfSettingsWindow_Close(object sender, RoutedEventArgs e)
        {
            ((ServerConfigVM)this.DataContext).ShowConfigSettingsWindow = false;
        }
        private async void opbNewConfigSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                configVM.IsSaving = true;
                cmConfig.ShowMessageProcess("Saving...");
                if (pbConfigPassword.Password == pbConfigPasswordRetype.Password)
                {

                    string newPasswordHash = null;
                    bool changePassword = pbConfigPassword.Password != Services.PasswordField;
                    if (changePassword) newPasswordHash = pbConfigPassword.Password.ToSha256();
                    bool Res = await Services.OPHttpClient.RequestJsonAsync<bool>(HttpMethod.Put, configVM.Host + "/config/saveutilitysettings", configVM.Username, configVM.Password, configVM.ToUtilityConfig(changePassword, newPasswordHash));                    
                    configVM.ConfigUsername = configVM.NewConfigUsername;
                    configVM.ConfigPasswordHash = newPasswordHash;
                    configVM.ConfigHTTPPort = configVM.NewConfigHTTPPort;
                    configVM.ConfigLocalhostOnly = configVM.NewConfigLocalhostOnly;
                    configVM.ConfigEnableHTTPS = configVM.NewConfigEnableHTTPS;
                    configVM.ConfigHTTPSPort = configVM.NewConfigHTTPSPort;
                    configVM.ConfigSSLCertificateSubjectName = configVM.NewConfigSSLCertificateSubjectName;
                    configVM.ConfigSSLCertificateStoreName = configVM.NewConfigSSLCertificateStoreName;
                    cmConfig.ShowMessageSuccess("Save Successful!");
                    await Task.Delay(1500);
                    if (Res) configVM.DisconnectClick.Execute(null); else { configVM.ShowConfigSettingsWindow = false; configVM.ShowConfigSelectSSLCertWindow = false; }
                }
                else cmConfig.ShowMessageError("'Password Retype' does not match 'Password'");
            }
            catch (Exception ex)
            {
                cmConfig.ShowMessageError("Save Failed!");
                MessageBox.Show(ex.ToString(), "Save Failed!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                configVM.IsSaving = false;
            }
        }

        //public Func<ServerConfigVM,Task<bool>> OnSaveAndApply;
        //public Func<string, string, string, Task<WebServerConfig>> OnRefresh;
        private async void opbLogin_Click(object sender, RoutedEventArgs e)
        {
            cmLogin.ShowMessageProcess("Logging in...");
            configVM.Host = optbHost.Text;
            configVM.IsLoggingIn = true;
            configVM.LoggedIn = false;
            configVM.Password = pbPassword.Password;
            uCHttpServerConfig.ucLogConsole.ClearLogs();
            try
            {
                //WebServerConfig newWebServerConfig = await this.OnRefresh?.Invoke(configVM.Host, configVM.Username, pbConfigPassword.Password);
                Tuple<WebServerConfig, SSLCertVM, SSLCertVM> obj = await Services.OPHttpClient.RequestJsonAsync<Tuple<WebServerConfig, SSLCertVM, SSLCertVM>>(HttpMethod.Get, configVM.Host + "/config/load", configVM.ConfigUsername, configVM.Password);
                WebServerConfig newWebServerConfig = obj.Item1;
                if (newWebServerConfig != null)
                {
                    configVM.Update(newWebServerConfig, obj.Item3.SubjectAlternativeNames);
                    pbConfigPassword.Password = Services.PasswordField;
                    pbConfigPasswordRetype.Password = Services.PasswordField;
                    ((HttpServerConfigVM)uCHttpServerConfig.DataContext).Update(newWebServerConfig,obj.Item2.SubjectAlternativeNames);

                    //configVM.HttpServerConfigVM.Update(newHttpServerConfig);
                    //configVM.HttpServerConfigVM = new HttpServerConfigVM(newHttpServerConfig);
                    //uCHttpServerConfig.DataContext = null;
                    //uCHttpServerConfig.DataContext = configVM.HttpServerConfigVM;

                    Settings settings = ((Settings)Services.Config);
                    settings.LastHost = configVM.Host;
                    settings.LastUsername = configVM.Username;
                    Services.SaveConfigFile("settings.json", settings);

                    httpServerConfigVM.ClientIsLocal = configVM.Host.Contains("localhost") || configVM.Host.Contains("127.0.0.1");
                    cmLogin.ShowMessageSuccess("Login Success!");
                    await Task.Delay(1500);
                    configVM.ShowLoginWindow = false;
                    configVM.LoggedIn = true;
                }
                else
                {
                    cmLogin.ShowMessageError("Login Failed!");
                    configVM.LoggedIn = false;
                }
            }
            catch
            {
                cmLogin.ShowMessageError("Login Failed!");
                configVM.LoggedIn = false;
            }
            finally
            {
                configVM.IsLoggingIn = false;
            }
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
        private async void opfConfigSelectSSLCert_OnShow(object sender, RoutedEventArgs e)
        {
            await ((SelectSSLCertVM)uCConfigSelectSSLCert.DataContext).Refresh();
        }


        private void opfConfigSelectSSLCert_OnClose(object sender, RoutedEventArgs e)
        {
            configVM.ShowConfigSelectSSLCertWindow = false;
        }

        private void UCConfigSelectSSLCert_OnCertificateSelected(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is SSLCertVM sSLCertVM)
            {
                configVM.NewConfigSSLCertificateSubjectName = sSLCertVM.Subject;
                configVM.NewConfigSSLCertificateStoreName = sSLCertVM.StoreName;
                configVM.NewConfigSubjectAlternativeNames.Clear();
                foreach (var s in sSLCertVM.SubjectAlternativeNames) configVM.NewConfigSubjectAlternativeNames.Add(s);
                configVM.ShowConfigSelectSSLCertWindow = false;
            }
        }

        private void miAdvanceSettings_Click(object sender, RoutedEventArgs e)
        {
            ((HttpServerConfigVM)uCHttpServerConfig.DataContext).ShowAdvanceSettingsClickCommand.Execute(null);
        }
    }
}
