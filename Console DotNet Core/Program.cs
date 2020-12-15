using Newtonsoft.Json;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using Vlix;


namespace Vlix.HttpServer
{
    partial class Program
    {
        static async Task Main(string[] args)
        {

            //HttpServerConfig config = new HttpServerConfig() { SSLCertificateSubjectName = "CN=azrin.vlix.me" };
            //config.Rules.Add(new HttpToHttpsRedirectRule("azrin.vlix.me"));
            //config.Rules.Add(new SimpleReverseProxyRule("azrin.vlix.me", "/rproxy/*", 5000));
            //config.Rules.Add(new SimplePathDenyRule("/forbidden.html"));
            //config.SaveConfigFile();
            WebServer webServer = new WebServer();
            await webServer.StartAsync();

            string[] wWWFiles = Directory.GetFiles(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Sample"));
            foreach (var wWWFile in wWWFiles) File.Copy(wWWFile, Path.Combine(webServer.WebServerConfig.WWWDirectoryParsed(), Path.GetFileName(wWWFile)), true);
            Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.Console().WriteTo.File(Path.Combine(webServer.WebServerConfig.LogDirectoryParsed(), "HTTPServer.log"), rollingInterval: RollingInterval.Day).CreateLogger();


            _ = Task.Run(async () =>
            {
                int n = 0;
                while (true)
                {
                    n++;
                    webServer.HttpServer.AddLog("This is Log " + n);
                    await Task.Delay(1000);
                }
            });

            Console.ReadKey();
        }
    }


}
