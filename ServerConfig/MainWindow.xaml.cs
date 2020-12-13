using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Vlix.HttpServer;
using Vlix.ServerConfigUI;

namespace ServerConfig
{

    public partial class MainWindow : Window
    {
        HttpClient httpClient;
        public MainWindow()
        {
            InitializeComponent();
            httpClient = new HttpClient();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {            

            uCServerConfig.OnRefresh = async (host, user, pass) =>
            {
                try
                {
                    HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, host + "/config/load");
                    requestMessage.Headers.Add("Authorization", "Basic " + (user + ":" + pass).ToBase64());
                    var res = await httpClient.SendAsync(requestMessage);
                    string bodyStr = await res.Content.ReadAsStringAsync();
                    WebServerConfig config = JsonConvert.DeserializeObject<WebServerConfig>(bodyStr);
                    return config;
                }
                catch (Exception ex)
                {
                    return null;
                }
            };
            uCServerConfig.OnSaveAndApply = async (serverConfigVM) =>
            {
                try
                {
                    HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Put, serverConfigVM.Host + "/config/save");
                    requestMessage.Headers.Add("Authorization", "Basic " + (serverConfigVM.ConfigUsername + ":" + serverConfigVM.Password).ToBase64());
                    requestMessage.Content = new StringContent(JsonConvert.SerializeObject(serverConfigVM.HttpServerConfigVM.ToModel()), Encoding.UTF8, "application/json");
                    var res = await httpClient.SendAsync(requestMessage);
                    return true;
                }
                catch
                {
                    return false;
                }
            };
        }
    }
}
