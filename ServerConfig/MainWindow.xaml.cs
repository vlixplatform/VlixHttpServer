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

    }
}
